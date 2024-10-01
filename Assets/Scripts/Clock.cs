using System;
using System.Threading;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.EventSystems;

public class Clock : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Transform hourHand;
    [SerializeField] private Transform minuteHand;
    [SerializeField] private Transform secondHand;

    [SerializeField] private Ease ease;
    [SerializeField] private float animationDuration;

    private bool isDragEnabled = false;
    private bool isDragging = false;
    private Transform currentHand;
    private DateTime dateTime;
    private DateTime lastSyncTime;
    private TimeSpan userOffset;

    private CancellationTokenSource cts;

    private void Start()
    {
        Init().Forget();
    }

    private async UniTask Init()
    {
        await SyncTimeWithServer();
        ShowTime(0f);

        cts = new CancellationTokenSource();
        UpdateClock(cts.Token).Forget();
    }

    public void ToggleDrag()
    {
        isDragEnabled = !isDragEnabled;
        if (isDragEnabled)
        {
            cts.Cancel();
        }
        else
        {
            if (!cts.IsCancellationRequested)
                cts.Cancel();
            cts = new CancellationTokenSource();
            UpdateClock(cts.Token).Forget();
        }
    }

    private void ShowTime(float duration)
    {
        var displayTime = dateTime + userOffset;

        var hours = displayTime.Hour % 12 + displayTime.Minute / 60f;
        var hourAngle = hours * 30f;

        var minutes = displayTime.Minute + displayTime.Second / 60f;
        var minuteAngle = minutes * 6f;

        var seconds = displayTime.Second;
        var secondAngle = seconds * 6f;

        hourHand.DOLocalRotate(new Vector3(0f, 0f, -hourAngle), duration).SetEase(ease);
        minuteHand.DOLocalRotate(new Vector3(0f, 0f, -minuteAngle), duration).SetEase(ease);
        secondHand.DOLocalRotate(new Vector3(0f, 0f, -secondAngle), duration).SetEase(ease);

        text.text = displayTime.ToString("HH:mm:ss");
    }

    private async UniTask UpdateClock(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (!isDragging)
            {
                ShowTime(animationDuration);
                var oldTime = DateTime.UtcNow;
                await UniTask.Delay(1000, DelayType.Realtime, PlayerLoopTiming.Update, token);
                var timeDifference = DateTime.UtcNow - oldTime;
                dateTime = dateTime.Add(timeDifference);
                if ((DateTime.UtcNow - lastSyncTime).TotalHours >= 1)
                    await SyncTimeWithServer();
            }
            else
            {
                await UniTask.Yield();
            }
        }
    }

    public void StartDraggingHand(Transform hand)
    {
        if (!isDragEnabled)
            return;
        isDragging = true;
        currentHand = hand;
    }

    public void DragHand(Transform hand, PointerEventData eventData)
    {
        if (!isDragging || currentHand != hand)
            return;

        var pointerDirection = eventData.position - (Vector2)hand.position;
        var pointerAngle = Vector2.SignedAngle(Vector2.up, pointerDirection);
        var handAngle = currentHand.localRotation.eulerAngles.z;
        var angleDiff = (pointerAngle - handAngle + 180 * 3) % 360 - 180;

        var timeDiff = new TimeSpan();
        if (currentHand == hourHand)
            timeDiff = TimeSpan.FromHours(-angleDiff / 30.0d);
        else if (currentHand == minuteHand)
            timeDiff = TimeSpan.FromMinutes(-angleDiff / 6.0d);
        else if (currentHand == secondHand)
            timeDiff = TimeSpan.FromSeconds(-angleDiff / 6.0d);
        userOffset = userOffset.Add(timeDiff);

        ShowTime(0);
    }


    public void StopDraggingHand()
    {
        isDragging = false;
        currentHand = null;
    }

    private async UniTask SyncTimeWithServer()
    {
        lastSyncTime = DateTime.UtcNow;
        dateTime = (await TimeFetcher.FetchTimeFromServer()).ToLocalTime();
    }
}