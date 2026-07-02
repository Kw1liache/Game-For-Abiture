using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class FloorTeleporter : MonoBehaviour
{
    [Header("Настройки телепорта")]
    // Этаж, на котором стоит ЭТОТ телепортер (выставь в инспекторе: 1, 2 или 3)
    [SerializeField] private int floorOfThisTeleporter = 1;
    [SerializeField] private int maxFloors = 3;

    // Текущий этаж игрока — ОБЩИЙ для всех телепортеров в сцене (статический)
    private static int currentFloor = 1;

    [Header("Точки телепортации")]
    [SerializeField] private Transform[] floorDestinations; // 3 точки (1, 2, 3 этаж)

    [Header("UI Тексты")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TextMeshProUGUI floorText;
    [SerializeField] private TextMeshProUGUI upText;
    [SerializeField] private TextMeshProUGUI downText;

    [Header("Настройки")]
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private CameraFollow cameraFollow;

    private Transform player;
    private bool isPlayerNear = false;

    // Идёт ли телепортация — общий флаг для всех телепортеров
    private static bool isTeleporting = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (cameraFollow == null)
            cameraFollow = FindFirstObjectByType<CameraFollow>();

        // 🚀 ФЕЙД ПАНЕЛЬ УДАЛЕНА ПОЛНОСТЬЮ

        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    void Update()
    {
        if (isTeleporting || player == null) return;

        // Этот телепортер реагирует ТОЛЬКО когда игрок находится на его этаже
        if (currentFloor != floorOfThisTeleporter)
        {
            if (isPlayerNear)
            {
                isPlayerNear = false;
                if (menuPanel != null) menuPanel.SetActive(false);
            }
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);
        bool wasNear = isPlayerNear;
        isPlayerNear = distance <= interactRange;

        if (isPlayerNear && !wasNear)
        {
            if (menuPanel != null) menuPanel.SetActive(true);
            UpdateUITexts();
        }

        if (!isPlayerNear && wasNear)
        {
            if (menuPanel != null) menuPanel.SetActive(false);
        }

        // F - Вверх
        if (isPlayerNear && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (currentFloor < maxFloors)
            {
                TeleportToFloor(currentFloor + 1);
            }
        }

        // V - Вниз
        if (isPlayerNear && Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
        {
            if (currentFloor > 1)
            {
                TeleportToFloor(currentFloor - 1);
            }
        }
    }

    void UpdateUITexts()
    {
        if (floorText != null)
            floorText.text = $"Этаж {currentFloor}";

        if (upText != null)
        {
            if (currentFloor < maxFloors)
                upText.text = $"Подняться на {currentFloor + 1} этаж [Нажмите F]";
            else
                upText.text = "Вы на последнем этаже!";
        }

        if (downText != null)
        {
            if (currentFloor > 1)
                downText.text = $"Спуститься на {currentFloor - 1} этаж [Нажмите V]";
            else
                downText.text = "Вы на первом этаже!";
        }
    }

    void TeleportToFloor(int targetFloor)
    {
        if (targetFloor < 1 || targetFloor > maxFloors) return;
        if (floorDestinations == null || floorDestinations.Length < targetFloor) return;

        StartCoroutine(TeleportCoroutine(targetFloor));
    }

    IEnumerator TeleportCoroutine(int targetFloor)
    {
        isTeleporting = true;
        if (menuPanel != null) menuPanel.SetActive(false);

        // 🚀 ЗАТЕМНЕНИЕ УБРАНО! Игрок просто мгновенно перемещается
        if (player != null)
        {
            player.position = floorDestinations[targetFloor - 1].position;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            currentFloor = targetFloor; // обновляем общий этаж для всех
        }

        // Обновляем камеру мгновенно
        if (cameraFollow != null)
            cameraFollow.SnapToPlayer();

        yield return null; // ждём 1 кадр, чтобы физика обновилась

        isTeleporting = false;

        // Сбрасываем близость — следующий Update нужного телепортера пересчитает её сам
        isPlayerNear = false;
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}