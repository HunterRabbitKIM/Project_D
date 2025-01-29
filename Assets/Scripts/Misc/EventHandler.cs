using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// �̺�Ʈ�� �����ϴ� ���� ���� Ŭ����
public static class EventHandler
{
    /// �� ��ȯ �̺�Ʈ (�Ű����� ����)
    /// �̺�Ʈ�� �����ϴ� ���� On���ٰ� �̸� ����
    /// �̺�Ʈ ȣ�� - Invoke �Լ� ���
    public static event Action OnSceneChangeEvent;

    // UI �Ŵ��� ��� ���� ȣ��Ǵ� �̺�Ʈ (�Ű����� ����)
    public static event Action AfterRegisterUIManager;

    // AfterRegisterUIManager �̺�Ʈ ȣ�� �޼���
    public static void CallAfterRegisterUIManager()
    {
        /// �̺�Ʈ�� �����ڰ� ���� ��쿡�� ȣ��
        /// Invoke �Լ��� ���� �̺�Ʈ�� ȣ���Ͽ� ������ ����� ���������� ����
        /// ?. �κ����� null�� �ƴϿ��� ������ �����ϰ� �ϴ� ��
        AfterRegisterUIManager?.Invoke();
    }
}
