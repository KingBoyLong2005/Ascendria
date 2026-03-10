// ReadyScene.cs
// Hiển thị stats từ ProfileCharacterData.GetRuntimeData() thay vì đọc thẳng base fields.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class ReadySceneManager : MonoBehaviour
{
    [Header("Character Selection")]
    [SerializeField] private List<ProfileCharacterData> availableCharacters = new List<ProfileCharacterData>();
    [SerializeField] private Button btnPreviousCharacter;
    [SerializeField] private Button btnNextCharacter;

    [Header("Character Preview (World Space)")]
    [Tooltip("Empty GameObject đặt ngoài Canvas, nơi model 3D sẽ spawn")]
    [SerializeField] private Transform characterPreviewRoot;

    [Header("Character Stats UI (TMP)")]
    [SerializeField] private TMP_Text txtCharacterName;
    [SerializeField] private TMP_Text txtMaxHP;
    [SerializeField] private TMP_Text txtAttack;
    [SerializeField] private TMP_Text txtArmor;
    [SerializeField] private TMP_Text txtMoveSpeed;
    [SerializeField] private TMP_Text txtLuck;
    [SerializeField] private TMP_Text txtWealth;
    [SerializeField] private TMP_Text txtWise;

    [Header("Map Selection - Dynamic")]
    [Tooltip("Container để spawn các map button (VD: GridLayout hoặc HorizontalLayout)")]
    [SerializeField] private Transform mapButtonContainer;
    [Tooltip("Prefab của button map (có Image component để hiển thị icon)")]
    [SerializeField] private GameObject mapButtonPrefab;
    [SerializeField] private TMP_Text txtSelectedMap;

    [Header("Panels")]
    [SerializeField] private GameObject panelStat;
    [SerializeField] private GameObject panelChooseMap;

    [Header("Navigation Buttons")]
    [SerializeField] private Button btnNext;
    [SerializeField] private Button btnBack;
    [SerializeField] private Button btnStartGame;

    [Header("Game Control")]
    [SerializeField] private string gameSceneName = "GameScene";

    // Runtime
    private int currentCharacterIndex = 0;
    private int selectedMapIndex = 0;
    private GameObject currentCharacterModel;
    private List<Button> mapButtons = new List<Button>();

    private void Start()
    {
        if (btnPreviousCharacter != null) btnPreviousCharacter.onClick.AddListener(PreviousCharacter);
        if (btnNextCharacter     != null) btnNextCharacter.onClick.AddListener(NextCharacter);
        if (btnNext              != null) btnNext.onClick.AddListener(ShowMapSelection);
        if (btnBack              != null) btnBack.onClick.AddListener(ShowCharacterSelection);
        if (btnStartGame         != null) btnStartGame.onClick.AddListener(StartGame);

        GenerateMapButtons();
        ShowCharacterSelection();

        if (availableCharacters.Count > 0)
            DisplayCharacter(0);
        else
            Debug.LogError("[ReadyScene] Chưa add ProfileCharacterData!");

        SelectMap(0);
    }

    // ═════════════════════════════════════════════
    // PANEL NAVIGATION
    // ═════════════════════════════════════════════

    private void ShowCharacterSelection()
    {
        if (panelStat      != null) panelStat.SetActive(true);
        if (panelChooseMap != null) panelChooseMap.SetActive(false);

        if (btnStartGame         != null) btnStartGame.gameObject.SetActive(false);
        if (btnBack              != null) btnBack.gameObject.SetActive(false);
        if (btnNext              != null) btnNext.gameObject.SetActive(true);
        if (btnNextCharacter     != null) btnNextCharacter.gameObject.SetActive(true);
        if (btnPreviousCharacter != null) btnPreviousCharacter.gameObject.SetActive(true);
    }

    private void ShowMapSelection()
    {
        if (panelStat      != null) panelStat.SetActive(false);
        if (panelChooseMap != null) panelChooseMap.SetActive(true);

        if (btnStartGame         != null) btnStartGame.gameObject.SetActive(true);
        if (btnNext              != null) btnNext.gameObject.SetActive(false);
        if (btnBack              != null) btnBack.gameObject.SetActive(true);
        if (btnNextCharacter     != null) btnNextCharacter.gameObject.SetActive(false);
        if (btnPreviousCharacter != null) btnPreviousCharacter.gameObject.SetActive(false);
    }

    // ═════════════════════════════════════════════
    // CHARACTER NAVIGATION
    // ═════════════════════════════════════════════

    private void NextCharacter()
    {
        if (availableCharacters.Count == 0) return;
        currentCharacterIndex = (currentCharacterIndex + 1) % availableCharacters.Count;
        DisplayCharacter(currentCharacterIndex);
    }

    private void PreviousCharacter()
    {
        if (availableCharacters.Count == 0) return;
        currentCharacterIndex = (currentCharacterIndex - 1 + availableCharacters.Count) % availableCharacters.Count;
        DisplayCharacter(currentCharacterIndex);
    }

    // ═════════════════════════════════════════════
    // DISPLAY CHARACTER
    // ═════════════════════════════════════════════

    private void DisplayCharacter(int index)
    {
        if (index < 0 || index >= availableCharacters.Count) return;

        ProfileCharacterData profile = availableCharacters[index];

        bool isUnlocked = GameSaveSystem.Instance.IsCharacterUnlocked(profile.displayName);

        if (txtCharacterName != null) txtCharacterName.text = profile.displayName;

        if (isUnlocked)
        {
            // TODO (Save System): BuildRuntimeData() sẽ cộng thêm upgrade stats ở đây
            ProfileCharacterData.RuntimeData data = profile.GetRuntimeData();

            if (txtMaxHP     != null) txtMaxHP.text     = data.maxHP.ToString("F0");
            if (txtAttack    != null) txtAttack.text    = data.attack.ToString("F1");
            if (txtArmor     != null) txtArmor.text     = data.armor.ToString("F1");
            if (txtMoveSpeed != null) txtMoveSpeed.text = data.moveSpeed.ToString("F1");
            if (txtLuck      != null) txtLuck.text      = data.luck.ToString("F1");
            if (txtWealth    != null) txtWealth.text    = data.wealth.ToString("F1");
            if (txtWise      != null) txtWise.text      = data.wise.ToString("F1");

            if (btnNext != null) btnNext.interactable = true;
        }
        else
        {
            // Chưa unlock: ẩn stats, hiện thông báo khoá
            string locked = "???";
            if (txtMaxHP     != null) txtMaxHP.text     = locked;
            if (txtAttack    != null) txtAttack.text    = locked;
            if (txtArmor     != null) txtArmor.text     = locked;
            if (txtMoveSpeed != null) txtMoveSpeed.text = locked;
            if (txtLuck      != null) txtLuck.text      = locked;
            if (txtWealth    != null) txtWealth.text    = locked;
            if (txtWise      != null) txtWise.text      = locked;

            // Khoá nút Next để không vào game với nhân vật chưa mở
            if (btnNext != null) btnNext.interactable = false;
        }

        SpawnPreviewModel(profile, isUnlocked);
    }

    private void SpawnPreviewModel(ProfileCharacterData character, bool isUnlocked = true)
    {
        if (currentCharacterModel != null)
            Destroy(currentCharacterModel);

        if (character.modelPrefab == null)
        {
            Debug.LogWarning($"[ReadyScene] {character.displayName} chưa có modelPrefab!");
            return;
        }
        if (characterPreviewRoot == null)
        {
            Debug.LogError("[ReadyScene] Chưa assign characterPreviewRoot!");
            return;
        }

        currentCharacterModel = Instantiate(
            character.modelPrefab,
            characterPreviewRoot.position,
            characterPreviewRoot.rotation
        );
        currentCharacterModel.transform.localScale = Vector3.one * 100;
        currentCharacterModel.transform.rotation   = Quaternion.Euler(0f, -180f, 0f);

        Animator animator = currentCharacterModel.GetComponentInChildren<Animator>();
        if (animator != null && character.animatorController != null)
        {
            animator.runtimeAnimatorController = character.animatorController;
            animator.SetBool("IsGrounded", true);
        }

        // Dim toàn bộ renderer nếu chưa unlock
        isUnlocked = GameSaveSystem.Instance.IsCharacterUnlocked(character.displayName);
        if (!isUnlocked)
        {
            foreach (var renderer in currentCharacterModel.GetComponentsInChildren<Renderer>())
            {
                foreach (var mat in renderer.materials)
                {
                    // Đổi màu sang tối (giữ texture, chỉ nhân color)
                    if (mat.HasProperty("_Color"))
                        mat.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                }
            }
        }
    }

    // ═════════════════════════════════════════════
    // MAP SELECTION
    // ═════════════════════════════════════════════

    private void GenerateMapButtons()
    {
        if (PrefabDatabase.Instance == null)  { Debug.LogError("[ReadyScene] PrefabDatabase not found!");            return; }
        if (mapButtonContainer      == null)  { Debug.LogError("[ReadyScene] mapButtonContainer chưa được assign!"); return; }
        if (mapButtonPrefab         == null)  { Debug.LogError("[ReadyScene] mapButtonPrefab chưa được assign!");    return; }

        List<MapData> maps = GetAvailableMaps();

        for (int i = 0; i < maps.Count; i++)
        {
            MapData mapData  = maps[i];
            int     mapIndex = i;

            GameObject buttonObj = Instantiate(mapButtonPrefab, mapButtonContainer);
            Button btn = buttonObj.GetComponent<Button>();
            if (btn == null)
            {
                Debug.LogError("[ReadyScene] Map Button Prefab không có Button component!");
                Destroy(buttonObj);
                continue;
            }

            Image iconImage = buttonObj.GetComponent<Image>();
            if (iconImage != null && mapData.icon != null)
                iconImage.sprite = mapData.icon;

            TMP_Text btnText = buttonObj.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
                btnText.text = mapData.mapName;

            btn.onClick.AddListener(() => SelectMap(mapIndex));
            mapButtons.Add(btn);
        }

        Debug.Log($"[ReadyScene] Generated {maps.Count} map buttons.");
    }

    private List<MapData> GetAvailableMaps()
    {
        var maps = new List<MapData>();
        var db = PrefabDatabase.Instance;
        if (db.firstMapPrefab  != null) maps.Add(new MapData { mapName = "Island",  mapPrefab = db.firstMapPrefab,  icon = db.firstMapIcon  });
        if (db.secondMapPrefab != null) maps.Add(new MapData { mapName = "Second Map", mapPrefab = db.secondMapPrefab, icon = db.secondMapIcon });
        if (db.thirdMapPrefab  != null) maps.Add(new MapData { mapName = "Third Map",  mapPrefab = db.thirdMapPrefab,  icon = db.thirdMapIcon  });
        return maps;
    }

    private void SelectMap(int mapIndex)
    {
        selectedMapIndex = mapIndex;

        if (txtSelectedMap != null)
        {
            List<MapData> maps = GetAvailableMaps();
            if (mapIndex >= 0 && mapIndex < maps.Count)
                txtSelectedMap.text = "Selected: " + maps[mapIndex].mapName;
        }

        UpdateMapButtonVisuals();
    }

    private void UpdateMapButtonVisuals()
    {
        for (int i = 0; i < mapButtons.Count; i++)
            ResetButtonColor(mapButtons[i]);

        if (selectedMapIndex >= 0 && selectedMapIndex < mapButtons.Count)
            HighlightButton(mapButtons[selectedMapIndex]);
    }

    private void HighlightButton(Button btn)
    {
        if (btn == null) return;

        // Reset màu về white (không bị tối)
        ColorBlock c = btn.colors;
        c.normalColor = Color.white;
        btn.colors = c;

        // Thêm hoặc bật Outline trắng
        Outline outline = btn.GetComponent<Outline>();
        if (outline == null)
            outline = btn.gameObject.AddComponent<Outline>();

        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(3f, -3f); // độ dày viền
        outline.enabled = true;
    }

    private void ResetButtonColor(Button btn)
    {
        if (btn == null) return;

        ColorBlock c = btn.colors;
        c.normalColor = Color.white;
        btn.colors = c;

        // Tắt outline
        Outline outline = btn.GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;
    }

    // ═════════════════════════════════════════════
    // START GAME
    // ═════════════════════════════════════════════

    private void StartGame()
    {
        if (availableCharacters.Count == 0)
        {
            Debug.LogError("[ReadyScene] Chưa có character!");
            return;
        }
        if (PrefabDatabase.Instance == null)
        {
            Debug.LogError("[ReadyScene] PrefabDatabase not found!");
            return;
        }

        ProfileCharacterData selected = availableCharacters[currentCharacterIndex];
        PrefabDatabase.Instance.SetSelectedCharacter(selected);
        PrefabDatabase.Instance.SetSelectedMap(selectedMapIndex);

        Debug.Log($"[ReadyScene] Start – Character: {selected.displayName}, Map: {selectedMapIndex}");
        SceneManager.LoadScene(gameSceneName);
    }

    // ═════════════════════════════════════════════
    // CLEANUP
    // ═════════════════════════════════════════════

    private void OnDestroy()
    {
        if (btnPreviousCharacter != null) btnPreviousCharacter.onClick.RemoveAllListeners();
        if (btnNextCharacter     != null) btnNextCharacter.onClick.RemoveAllListeners();
        if (btnNext              != null) btnNext.onClick.RemoveAllListeners();
        if (btnBack              != null) btnBack.onClick.RemoveAllListeners();
        if (btnStartGame         != null) btnStartGame.onClick.RemoveAllListeners();

        foreach (Button btn in mapButtons)
            if (btn != null) btn.onClick.RemoveAllListeners();

        if (currentCharacterModel != null)
            Destroy(currentCharacterModel);
    }
}

// ═════════════════════════════════════════════
// MAP DATA STRUCT
// ═════════════════════════════════════════════

[System.Serializable]
public class MapData
{
    public string     mapName;
    public GameObject mapPrefab;
    public Sprite     icon;
}