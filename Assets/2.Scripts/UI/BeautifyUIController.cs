using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using BeautifyEffect = Beautify.Universal.Beautify;

public class BeautifyUIController : MonoBehaviour
{
    [Header("Blur Settings")]
    [SerializeField] private float startBlur = 0f;    // 추가: 시작 블러 값 (인스펙터에서 조절 가능)
    [SerializeField] private float endBlur = 1.4f;    // 추가: 종료 블러 값 (인스펙터에서 조절 가능)

    [Header("Blink Settings")]
    [SerializeField] private int blinkCount = 3;
    [SerializeField] private float blinkSpeed = 0.8f; // 기본값을 0.5초로 증가
    [SerializeField] private float blinkFadeDuration = 0.4f; // 페이드 지속시간 추가
    [SerializeField] private float blinkInterval = 0.3f; // 깜빡임 간격 추가
    [SerializeField] private float initialDelay = 0.2f; // 첫 깜빡임 전 초기 대기시간 추가

    [SerializeField] private Volume volume;           // 추가: Volume 컴포넌트 참조

    private BeautifyEffect beautify;                  // 추가: Beautify 이펙트 참조
    private float originalBlur;                       // 추가: 원본 블러 값 저장
    private bool isBlinking = false;                  // 추가: 깜빡임 중인지 확인하는 플래그

    public static event System.Action OnEffectStarted;
    public static event System.Action OnEffectCompleted;

    // 추가: 초기화 함수
    private void Start()
    {
        StartCoroutine(DelayedInitialize());
    }

    private IEnumerator DelayedInitialize()
    {
        // 1프레임 대기
        yield return null;

        // Volume과 Beautify가 준비될 때까지 재시도
        int maxAttempts = 10;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            if (InitializeBeautify())
            {
                break;
            }

            attempts++;
            Debug.LogWarning($"Beautify 초기화 재시도 {attempts}/{maxAttempts}");
            yield return new WaitForSeconds(0.1f);
        }

        if (attempts >= maxAttempts)
        {
            Debug.LogError("Beautify 초기화 실패!");
        }
    }

    // 추가: Beautify 초기화 함수
    private bool InitializeBeautify()
    {
        if (volume == null)
        {
            volume = FindObjectOfType<Volume>();
            if (volume == null)
            {
                Debug.LogWarning("Volume을 찾을 수 없습니다!");
                return false;
            }
        }

        if (volume.profile == null)
        {
            Debug.LogWarning("Volume Profile이 null입니다!");
            return false;
        }

        if (volume.profile.TryGet(out beautify))
        {
            // Blur 설정 초기화
            beautify.blurIntensity.overrideState = true;
            originalBlur = beautify.blurIntensity.value;

            // Vignette 설정 초기화
            beautify.vignettingBlink.overrideState = true;
            beautify.vignettingBlink.value = 0f;

            return true;
        }
        else
        {
            Debug.LogWarning("Beautify 컴포넌트를 Volume Profile에서 찾을 수 없습니다!");
            return false;
        }
    }

    // 추가: 시작 블러 적용 함수
    public void ApplyStartBlur()
    {
        if (beautify != null)
        {
            beautify.blurIntensity.value = startBlur;
        }
    }

    // 추가: 종료 블러 적용 함수
    public void ApplyEndBlur()
    {
        if (beautify != null)
        {
            beautify.blurIntensity.value = endBlur;
        }
    }

    // 추가: 원본 블러로 복원하는 함수
    public void RestoreOriginalBlur()
    {
        if (beautify != null)
        {
            beautify.blurIntensity.value = originalBlur;
        }
    }

    // 추가: 눈 깜빡임 실행 함수
    public void StartBlinking()
    {
        if (!isBlinking && beautify != null)
        {
            OnEffectStarted?.Invoke();
            StartCoroutine(BlinkCoroutineWithNotification());
        }
    }

    private IEnumerator BlinkCoroutineWithNotification()
    {
        yield return StartCoroutine(BlinkCoroutine());

        Debug.Log("Blink 이펙트 완료");
        OnEffectCompleted?.Invoke();
    }

    // 추가: 눈 깜빡임 코루틴
    private IEnumerator BlinkCoroutine()
    {
        isBlinking = true;

        yield return new WaitForSeconds(initialDelay);

        for (int i = 0; i < blinkCount; i++)
        {
            // 자연스러운 페이드 인 (0 -> 1)
            yield return StartCoroutine(FadeVignette(0f, 1f, blinkFadeDuration));

            // 자연스러운 페이드 아웃 (1 -> 0)
            yield return new WaitForSeconds(blinkSpeed * 0.4f);

            yield return StartCoroutine(FadeVignette(1f, 0f, blinkFadeDuration));

            // 깜빡임 간격
            if (i < blinkCount - 1) // 마지막 깜빡임이 아닐 때만
            {
                yield return new WaitForSeconds(blinkInterval);
            }
        }

        isBlinking = false;
    }

    private IEnumerator FadeVignette(float startValue, float endValue, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Ease-in-out 곡선 적용
            t = Mathf.SmoothStep(0f, 1f, t);

            if (beautify != null)
            {
                beautify.vignettingBlink.value = Mathf.Lerp(startValue, endValue, t);
            }

            yield return null;
        }

        if (beautify != null)
        {
            beautify.vignettingBlink.value = endValue;
        }
    }

    // 추가: 깜빡임 중지 함수
    public void StopBlinking()
    {
        if (isBlinking)
        {
            StopAllCoroutines();
            isBlinking = false;
            if (beautify != null)
            {
                beautify.vignettingBlink.value = 0f;
            }
        }
    }

    public void ExecuteBlinkWithBlur(System.Action onComplete = null)
    {
        OnEffectStarted?.Invoke(); // 이펙트 시작 알림
        StartCoroutine(BlinkWithBlurCoroutine(onComplete));
    }

    private IEnumerator BlinkWithBlurCoroutine(System.Action onComplete = null)
    {
        isBlinking = true;
        // 깜빡임 실행
        ApplyStartBlur();
        ApplyEndBlur();
        yield return StartCoroutine(BlinkCoroutine());

        // 깜빡임 완료 후 잠깐 대기
        yield return new WaitForSeconds(0.3f);
        
        // 블러를 부드럽게 원래 값으로 복원
        yield return StartCoroutine(FadeBlur(beautify.blurIntensity.value, originalBlur, 0.5f));

        OnEffectCompleted?.Invoke(); // 이펙트 완료 알림
        onComplete?.Invoke();
    }

    private IEnumerator FadeBlur(float startBlur, float endBlur, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            beautify.blurIntensity.value = Mathf.Lerp(startBlur, endBlur, t);
            yield return null;
        }

        beautify.blurIntensity.value = endBlur;
    }

    public bool IsBlinking()
    {
        return isBlinking;
    }

    public float GetTotalEffectDuration()
    {
        // 깜빡임 시간 + 블러 복원 시간 + 추가 대기시간 계산
        float blinkDuration = (blinkFadeDuration * 2 + blinkSpeed * 0.3f) * blinkCount + blinkInterval * (blinkCount - 1);
        float blurFadeDuration = 0.5f;
        float additionalWait = 0.3f;

        return blinkDuration + additionalWait + blurFadeDuration;
    }

    // 추가: 모든 이펙트 초기화 함수
    public void ResetAllEffects()
    {
        StopBlinking();
        RestoreOriginalBlur();
    }

    public void SetCustomBlurValues(float customStart, float customEnd)
    {
        startBlur = customStart;
        endBlur = customEnd;
    }

    public void SetCustomBlinkSettings(int count, float speed)
    {
        blinkCount = count;
        blinkSpeed = speed;
    }
}
