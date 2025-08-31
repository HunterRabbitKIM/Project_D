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
    [SerializeField] private int blinkCount = 3;      // 추가: 깜빡임 횟수 (인스펙터에서 조절 가능)
    [SerializeField] private float blinkSpeed = 0.2f; // 추가: 깜빡임 속도

    [SerializeField] private Volume volume;           // 추가: Volume 컴포넌트 참조

    private BeautifyEffect beautify;                  // 추가: Beautify 이펙트 참조
    private float originalBlur;                       // 추가: 원본 블러 값 저장
    private bool isBlinking = false;                  // 추가: 깜빡임 중인지 확인하는 플래그

    // 추가: 초기화 함수
    private void Start()
    {
        InitializeBeautify();
    }

    // 추가: Beautify 초기화 함수
    private void InitializeBeautify()
    {
        if (volume != null && volume.profile.TryGet(out beautify))
        {
            // Blur 설정 초기화
            beautify.blurIntensity.overrideState = true;
            originalBlur = beautify.blurIntensity.value;
            
            // Vignette 설정 초기화
            beautify.vignettingBlink.overrideState = true;
            beautify.vignettingBlink.value = 0f;
        }
        else
        {
            Debug.LogError("Volume 또는 Beautify 프로파일을 찾을 수 없습니다!");
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
            StartCoroutine(BlinkCoroutine());
        }
    }

    // 추가: 눈 깜빡임 코루틴
    private IEnumerator BlinkCoroutine()
    {
        isBlinking = true;
        
        for (int i = 0; i < blinkCount; i++)
        {
            // Vignette Blink를 1로 설정 (깜빡임 시작)
            beautify.vignettingBlink.value = 1f;
            yield return new WaitForSeconds(blinkSpeed);
            
            // Vignette Blink를 0으로 설정 (깜빡임 끝)
            beautify.vignettingBlink.value = 0f;
            yield return new WaitForSeconds(blinkSpeed);
        }
        
        isBlinking = false;
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

    // 추가: 복합 이펙트 함수 - 블러와 깜빡임을 동시에 실행
    public void ExecuteBlinkWithBlur()
    {
        ApplyEndBlur();
        StartBlinking();
    }

    // 추가: 모든 이펙트 초기화 함수
    public void ResetAllEffects()
    {
        StopBlinking();
        RestoreOriginalBlur();
    }
}
