using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHudCanvas : MonoBehaviour
{
    public static bool IsEnabled { get; private set; }

    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerCombat playerCombat;

    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup hudGroup;
    [SerializeField] private RectTransform healthFill;
    [SerializeField] private RectTransform healthFlash;
    [SerializeField] private RectTransform manaFill;
    [SerializeField] private RectTransform manaFlash;
    [SerializeField] private Image spellIcon;
    [SerializeField] private RectTransform cooldownMask;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private GameObject loadSlotsPanel;
    private float lastHealth = -1f;
    private float flashHealth;
    private float healthFlashTimer;
    private float lastMana = -1f;
    private float flashMana;
    private float manaFlashTimer;
    private bool returningToMainMenu;

    [SerializeField] private Sprite[] spellIcons;
    [SerializeField] private Button loadGameButton, mainMenuButton, quitButton, backButton;
    [SerializeField] private Button[] slotButtons;

    private const float FlashDuration = 0.75f;

    public void SetPlayerStats(PlayerStats stats)
    {
        playerStats = stats;
        if (playerStats != null)
        {
            playerCombat = playerStats.GetComponent<PlayerCombat>();
        }
    }

    private void Awake()
    {
        IsEnabled = true;
        if (canvas == null || hudGroup == null || deathPanel == null)
        { Debug.LogError("HUD prefab has missing references.", this); enabled = false; return; }
        BindButtons();
    }

    private void OnDestroy()
    {
        IsEnabled = false;
    }

    private void Update()
    {
        if (HomeMode.IsActive && (playerStats == null || !playerStats.IsDead))
        {
            canvas.enabled = false;
            return;
        }

        canvas.enabled = true;
        ResolvePlayer();
        if (playerStats == null)
        {
            return;
        }

        hudGroup.alpha = PauseMenu.IsPaused ? 0.3f : 1f;
        UpdateResourceFlashes();
        UpdateBars();
        UpdateSpellIcon();
        UpdateDeathMenu();
    }

    private void ResolvePlayer()
    {
        if (playerStats != null)
        {
            return;
        }

        GameObject player = SystemsBootstrap.Instance != null ? SystemsBootstrap.Instance.Player : GameObject.Find("Player");
        if (player == null)
        {
            return;
        }

        playerStats = player.GetComponent<PlayerStats>();
        playerCombat = player.GetComponent<PlayerCombat>();
    }

    private void BindButtons()
    {
        loadGameButton.onClick.AddListener(() => ShowLoadSlots(true));
        mainMenuButton.onClick.AddListener(() => StartCoroutine(ReturnToMainMenuRoutine()));
        quitButton.onClick.AddListener(QuitGame);
        for (int slot = 1; slot <= SaveSystem.SlotCount; slot++)
        { int index = slot; slotButtons[slot - 1].onClick.AddListener(() => LoadSlot(index)); }
        backButton.onClick.AddListener(() => ShowLoadSlots(false));
    }
#if UNITY_EDITOR
    private void BuildCanvas()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        EnsureEventSystem();

        Transform existing = transform.Find("HudRoot");
        if (existing != null)
        {
            return;
        }

        RectTransform hudRoot = CreateRect("HudRoot", transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        hudRoot.anchoredPosition = Vector2.zero;
        hudGroup = hudRoot.gameObject.AddComponent<CanvasGroup>();

        CreateResourceBar(hudRoot, "Health", "UI/HealthBarUI", new Vector2(215f, -70f), Color.red, out healthFill, out healthFlash);
        CreateResourceBar(hudRoot, "Mana", "UI/ManaBarUI", new Vector2(215f, -180f), Color.blue, out manaFill, out manaFlash);
        CreateSpellSelector(hudRoot);
        CreateDeathPanel();
    }

    private void CreateResourceBar(RectTransform parent, string name, string framePath, Vector2 position, Color color, out RectTransform fill, out RectTransform flash)
    {
        RectTransform root = CreateRect(name + "Bar", parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0.5f, 0.5f));
        root.sizeDelta = new Vector2(390f, 130f);
        root.anchoredPosition = position;

        RectTransform fillArea = CreateRect("FillArea", root, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f));
        fillArea.offsetMin = new Vector2(86f, 55f);
        fillArea.offsetMax = new Vector2(-56f, -61f);

        Image fillImage = CreateImage("Fill", fillArea, color);
        fill = fillImage.rectTransform;
        fill.anchorMin = new Vector2(0f, 0f);
        fill.anchorMax = new Vector2(0f, 1f);
        fill.pivot = new Vector2(0f, 0.5f);
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;

        Image flashImage = CreateImage("SpendFlash", fillArea, Color.white);
        flash = flashImage.rectTransform;
        flash.anchorMin = new Vector2(0f, 0f);
        flash.anchorMax = new Vector2(0f, 1f);
        flash.pivot = new Vector2(0f, 0.5f);
        flash.offsetMin = Vector2.zero;
        flash.offsetMax = Vector2.zero;

        Image frame = CreateImage("Frame", root, Color.white);
        frame.sprite = CreateFrameSprite(framePath);
        frame.raycastTarget = false;
        Stretch(frame.rectTransform);
    }

    private void CreateSpellSelector(RectTransform parent)
    {
        RectTransform root = CreateRect("SpellSelector", parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0.5f, 0.5f));
        root.sizeDelta = new Vector2(72f, 72f);
        root.anchoredPosition = new Vector2(470f, -140f);

        CreateImage("Background", root, new Color(0f, 0f, 0f, 0.85f)).rectTransform.sizeDelta = root.sizeDelta;
        spellIcon = CreateImage("Icon", root, Color.white);
        spellIcon.rectTransform.sizeDelta = new Vector2(44f, 44f);
        spellIcon.raycastTarget = false;

        Image cooldown = CreateImage("Cooldown", root, new Color(0.45f, 0.45f, 0.45f, 0.72f));
        cooldownMask = cooldown.rectTransform;
        cooldownMask.anchorMin = new Vector2(0f, 0f);
        cooldownMask.anchorMax = new Vector2(1f, 0f);
        cooldownMask.pivot = new Vector2(0.5f, 0f);
        cooldownMask.offsetMin = Vector2.zero;
        cooldownMask.offsetMax = new Vector2(0f, 0f);
    }

    private void CreateDeathPanel()
    {
        deathPanel = new GameObject("DeathPanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        deathPanel.transform.SetParent(transform, false);
        Image background = deathPanel.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.78f);
        Stretch(deathPanel.GetComponent<RectTransform>());

        Text title = CreateText("Title", deathPanel.transform, "GAME OVER", 66, TextAnchor.MiddleCenter);
        title.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        title.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        title.rectTransform.pivot = new Vector2(0.5f, 1f);
        title.rectTransform.anchoredPosition = new Vector2(0f, -70f);
        title.rectTransform.sizeDelta = new Vector2(900f, 110f);

        loadGameButton = CreateButton("Load Game", new Vector2(0f, 70f), new Color(0.2f, 0.45f, 0.2f, 0.92f), () => ShowLoadSlots(true));
        mainMenuButton = CreateButton("Main Menu", new Vector2(0f, -55f), new Color(0.52f, 0.45f, 0.12f, 0.92f), () => StartCoroutine(ReturnToMainMenuRoutine()));
        quitButton = CreateButton("Quit to Desktop", new Vector2(0f, -180f), new Color(0.48f, 0.16f, 0.16f, 0.92f), QuitGame);

        loadSlotsPanel = new GameObject("LoadSlots", typeof(RectTransform));
        loadSlotsPanel.transform.SetParent(deathPanel.transform, false);
        RectTransform slotsRect = loadSlotsPanel.GetComponent<RectTransform>();
        slotsRect.anchorMin = new Vector2(0.5f, 0.5f);
        slotsRect.anchorMax = new Vector2(0.5f, 0.5f);
        slotsRect.pivot = new Vector2(0.5f, 0.5f);
        slotsRect.sizeDelta = new Vector2(760f, 430f);
        slotsRect.anchoredPosition = new Vector2(0f, -40f);

        slotButtons = new Button[SaveSystem.SlotCount];
        for (int slot = 1; slot <= SaveSystem.SlotCount; slot++)
        {
            int loadSlot = slot;
            Button button = CreateButton(loadSlotsPanel.transform, "Slot " + slot, new Vector2(0f, 190f - slot * 72f), new Vector2(620f, 58f), new Color(0.18f, 0.34f, 0.18f, 0.92f), () => LoadSlot(loadSlot));
            slotButtons[slot - 1] = button;
            button.GetComponentInChildren<Text>().text = SaveSystem.GetSlotSummary(slot);
        }

        backButton = CreateButton(loadSlotsPanel.transform, "Back", new Vector2(0f, -220f), new Vector2(260f, 64f), new Color(0.48f, 0.16f, 0.16f, 0.92f), () => ShowLoadSlots(false));
        ShowLoadSlots(false);
        deathPanel.SetActive(false);
    }

    private Button CreateButton(string label, Vector2 position, Color color, UnityEngine.Events.UnityAction action)
    {
        return CreateButton(deathPanel.transform, label, position, new Vector2(620f, 76f), color, action);
    }

    private Button CreateButton(Transform parent, string label, Vector2 position, Vector2 size, Color color, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image = buttonObject.GetComponent<Image>();
        image.color = color;

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(action);

        Text text = CreateText("Text", buttonObject.transform, label, 34, TextAnchor.MiddleCenter);
        Stretch(text.rectTransform);
        return button;
    }

#endif
    private void UpdateBars()
    {
        SetFill(healthFill, playerStats.Health, playerStats.MaxHealth);
        SetFlash(healthFlash, playerStats.Health, playerStats.MaxHealth, flashHealth, healthFlashTimer);
        SetFill(manaFill, playerStats.Mana, playerStats.MaxMana);
        SetFlash(manaFlash, playerStats.Mana, playerStats.MaxMana, flashMana, manaFlashTimer);
    }

    private void UpdateSpellIcon()
    {
        if (playerCombat == null)
        {
            playerCombat = playerStats.GetComponent<PlayerCombat>();
        }

        if (playerCombat == null)
        {
            return;
        }

        spellIcon.color = playerCombat.EquippedSpellColor;
        if (spellIcons != null && (int)playerCombat.EquippedSpell < spellIcons.Length) spellIcon.sprite = spellIcons[(int)playerCombat.EquippedSpell];
        float cooldown = playerCombat.CooldownRemainingPercent;
        cooldownMask.anchorMax = new Vector2(1f, cooldown);
    }

    private void UpdateResourceFlashes()
    {
        TrackFlash(playerStats.Health, ref lastHealth, ref flashHealth, ref healthFlashTimer);
        TrackFlash(playerStats.Mana, ref lastMana, ref flashMana, ref manaFlashTimer);
    }

    private static void TrackFlash(float current, ref float last, ref float flashValue, ref float timer)
    {
        if (last < 0f)
        {
            last = current;
            flashValue = current;
        }

        if (current < last)
        {
            flashValue = last;
            timer = FlashDuration;
        }
        else if (current > last)
        {
            flashValue = current;
        }

        last = current;
        if (timer > 0f)
        {
            timer = Mathf.Max(0f, timer - Time.unscaledDeltaTime);
            flashValue = Mathf.Lerp(current, flashValue, timer / FlashDuration);
        }
    }

    private void UpdateDeathMenu()
    {
        bool dead = playerStats.IsDead;
        deathPanel.SetActive(dead);
        if (dead)
        {
            GameModal.Open();
            Time.timeScale = 0f;
            RefreshLoadButtons();
        }
    }

    private void RefreshLoadButtons()
    {
        if (loadSlotsPanel == null)
        {
            return;
        }

        Button[] buttons = loadSlotsPanel.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < SaveSystem.SlotCount && i < buttons.Length; i++)
        {
            int slot = i + 1;
            buttons[i].interactable = SaveSystem.HasSave(slot);
            buttons[i].GetComponentInChildren<Text>().text = SaveSystem.GetSlotSummary(slot);
        }
    }

    private void ShowLoadSlots(bool show)
    {
        loadSlotsPanel.SetActive(show);
        loadGameButton.gameObject.SetActive(!show);
        mainMenuButton.gameObject.SetActive(!show);
        quitButton.gameObject.SetActive(!show);
    }

    private void LoadSlot(int slot)
    {
        SaveData save = SaveSystem.ReadSlot(slot);
        if (save == null)
        {
            return;
        }

        GameModal.Close();
        Time.timeScale = 1f;
        AudioListener.pause = false;
        GameSession.LoadFromSave(save);
    }

    private IEnumerator ReturnToMainMenuRoutine()
    {
        if (returningToMainMenu)
        {
            yield break;
        }

        returningToMainMenu = true;
        GameModal.Close();
        Time.timeScale = 1f;
        AudioListener.pause = false;
        // Release the persistent level manager before MainMenu loads. Otherwise
        // it treats MainMenu's authored camera as a duplicate and destroys it.
        SystemsBootstrap.Teardown();
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        yield break;
    }

    private static void QuitGame()
    {
        GameModal.Close();
        Time.timeScale = 1f;
        AudioListener.pause = false;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static void SetFill(RectTransform fill, float value, float maxValue)
    {
        float percent = maxValue > 0f ? Mathf.Clamp01(value / maxValue) : 0f;
        fill.anchorMax = new Vector2(percent, 1f);
    }

    private static void SetFlash(RectTransform flash, float value, float maxValue, float flashValue, float timer)
    {
        if (timer <= 0f || maxValue <= 0f)
        {
            flash.anchorMin = Vector2.zero;
            flash.anchorMax = Vector2.zero;
            return;
        }

        float currentPercent = Mathf.Clamp01(value / maxValue);
        float flashPercent = Mathf.Clamp01(flashValue / maxValue);
        flash.anchorMin = new Vector2(currentPercent, 0f);
        flash.anchorMax = new Vector2(Mathf.Max(currentPercent, flashPercent), 1f);
    }

#if UNITY_EDITOR
    private Image CreateImage(string name, Transform parent, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        Image image = obj.GetComponent<Image>();
        image.color = color;
        image.sprite = ShapeSprites.Square;
        image.type = Image.Type.Simple;
        image.raycastTarget = false;
        return image;
    }

    private Text CreateText(string name, Transform parent, string value, int fontSize, TextAnchor anchor)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent, false);
        Text text = obj.GetComponent<Text>();
        text.text = value;
        text.font = Resources.Load<Font>("Fonts/CoralinesCat") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.color = Color.white;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 14;
        text.resizeTextMaxSize = fontSize;
        return text;
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Sprite CreateFrameSprite(string resourcePath)
    {
        Texture2D source = Resources.Load<Texture2D>(resourcePath);
        if (source == null)
        {
            return ShapeSprites.Square;
        }

        Color[] pixels = source.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            if (IsUiBackground(pixels[i]))
            {
                pixels[i] = Color.clear;
            }
        }

        Texture2D texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        texture.SetPixels(pixels);
        texture.Apply(false, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
    }

    private static bool IsUiBackground(Color color)
    {
        bool veryLight = color.r > 0.78f && color.g > 0.78f && color.b > 0.78f;
        float channelSpread = Mathf.Max(color.r, Mathf.Max(color.g, color.b)) - Mathf.Min(color.r, Mathf.Min(color.g, color.b));
        return veryLight && channelSpread < 0.18f;
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
        // EventSystem is saved with the rig.
    }
#endif
}
