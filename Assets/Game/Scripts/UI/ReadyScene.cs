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
        // Character selection buttons
        if (btnPreviousCharacter != null)
            btnPreviousCharacter.onClick.AddListener(PreviousCharacter);

        if (btnNextCharacter != null)
            btnNextCharacter.onClick.AddListener(NextCharacter);

        // Navigation buttons
        if (btnNext != null)
            btnNext.onClick.AddListener(ShowMapSelection);

        if (btnBack != null)
            btnBack.onClick.AddListener(ShowCharacterSelection);

        if (btnStartGame != null)
            btnStartGame.onClick.AddListener(StartGame);

        // Tạo map buttons từ PrefabDatabase
        GenerateMapButtons();

        // Mở panel character đầu tiên
        ShowCharacterSelection();

        if (availableCharacters.Count > 0)
            DisplayCharacter(0);
        else
            Debug.LogError("[ReadyScene] Chua add ProfileCharacterData!");

        SelectMap(0);
    }

    // ═════════════════════════════════════════════
    // PANEL NAVIGATION
    // ═════════════════════════════════════════════

    private void ShowCharacterSelection()
    {
        if (panelStat != null)
            panelStat.SetActive(true);

        if (panelChooseMap != null)
            panelChooseMap.SetActive(false);

        if (btnStartGame != null)
            btnStartGame.gameObject.SetActive(false);
        if(btnBack != null)
            btnBack.gameObject.SetActive(false);
        if(btnNext != null)
            btnNext.gameObject.SetActive(true);
        if(btnNextCharacter != null)
            btnNextCharacter.gameObject.SetActive(true);
        if(btnPreviousCharacter != null)
            btnPreviousCharacter.gameObject.SetActive(true);
    }

    private void ShowMapSelection()
    {
        if (panelStat != null)
            panelStat.SetActive(false);

        if (panelChooseMap != null)
            panelChooseMap.SetActive(true);

        if (btnStartGame != null)
            btnStartGame.gameObject.SetActive(true);
        if(btnNext != null)
            btnNext.gameObject.SetActive(false);
        if(btnBack != null)
            btnBack.gameObject.SetActive(true);
        if(btnNextCharacter != null)
            btnNextCharacter.gameObject.SetActive(false);
        if(btnPreviousCharacter != null)
            btnPreviousCharacter.gameObject.SetActive(false);
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
        currentCharacterIndex--;
        if (currentCharacterIndex < 0)
            currentCharacterIndex = availableCharacters.Count - 1;
        DisplayCharacter(currentCharacterIndex);
    }

    // ═════════════════════════════════════════════
    // DISPLAY CHARACTER
    // ═════════════════════════════════════════════

    private void DisplayCharacter(int index)
    {
        if (index < 0 || index >= availableCharacters.Count) return;

        ProfileCharacterData character = availableCharacters[index];

        if (txtCharacterName != null) txtCharacterName.text = character.displayName;
        if (txtMaxHP != null)         txtMaxHP.text = character.maxHP.ToString("F0");
        if (txtAttack != null)        txtAttack.text = character.CharacterAttack.ToString("F1");
        if (txtArmor != null)         txtArmor.text = character.Armor.ToString("F1");
        if (txtMoveSpeed != null)     txtMoveSpeed.text = character.MoveSpeed.ToString("F1");
        if (txtLuck != null)          txtLuck.text = character.Luck.ToString("F1");
        if (txtWealth != null)        txtWealth.text = character.Wealth.ToString("F1");
        if (txtWise != null)          txtWise.text = character.Wise.ToString("F1");

        SpawnPreviewModel(character);
    }

    private void SpawnPreviewModel(ProfileCharacterData character)
    {
        if (currentCharacterModel != null)
            Destroy(currentCharacterModel);

        if (character.modelPrefab == null)
        {
            Debug.LogWarning($"[ReadyScene] {character.displayName} chua co modelPrefab!");
            return;
        }
        if (characterPreviewRoot == null)
        {
            Debug.LogError("[ReadyScene] Chua assign characterPreviewRoot!");
            return;
        }

        currentCharacterModel = Instantiate(
            character.modelPrefab,
            characterPreviewRoot.position,
            characterPreviewRoot.rotation
        );

        currentCharacterModel.transform.localScale = Vector3.one * 100;
        currentCharacterModel.transform.rotation = Quaternion.Euler(new Vector3(0, -180, 0));

        Animator animator = currentCharacterModel.GetComponentInChildren<Animator>();
        if (animator != null && character.animatorController != null)
        {
            animator.runtimeAnimatorController = character.animatorController;
            animator.SetBool("IsGrounded", true);
        }
    }

    // ═════════════════════════════════════════════
    // MAP SELECTION - DYNAMIC GENERATION
    // ═════════════════════════════════════════════

    private void GenerateMapButtons()
    {
        if (PrefabDatabase.Instance == null)
        {
            Debug.LogError("[ReadyScene] PrefabDatabase not found!");
            return;
        }

        if (mapButtonContainer == null)
        {
            Debug.LogError("[ReadyScene] Map Button Container chua duoc assign!");
            return;
        }

        if (mapButtonPrefab == null)
        {
            Debug.LogError("[ReadyScene] Map Button Prefab chua duoc assign!");
            return;
        }

        // Lấy danh sách map từ PrefabDatabase
        List<MapData> maps = GetAvailableMaps();

        for (int i = 0; i < maps.Count; i++)
        {
            MapData mapData = maps[i];
            int mapIndex = i; // Capture index for closure

            // Spawn button từ prefab
            GameObject buttonObj = Instantiate(mapButtonPrefab, mapButtonContainer);
            Button btn = buttonObj.GetComponent<Button>();

            if (btn == null)
            {
                Debug.LogError("[ReadyScene] Map Button Prefab khong co Button component!");
                Destroy(buttonObj);
                continue;
            }

            // Set icon nếu có
            Image iconImage = buttonObj.GetComponent<Image>();
            if (iconImage != null && mapData.icon != null)
            {
                iconImage.sprite = mapData.icon;
            }

            // Set text nếu có TMP_Text child
            TMP_Text btnText = buttonObj.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
            {
                btnText.text = mapData.mapName;
            }

            // Add listener
            btn.onClick.AddListener(() => SelectMap(mapIndex));

            mapButtons.Add(btn);
        }

        Debug.Log($"[ReadyScene] Generated {maps.Count} map buttons.");
    }

    private List<MapData> GetAvailableMaps()
    {
        List<MapData> maps = new List<MapData>();

        if (PrefabDatabase.Instance.firstMapPrefab != null)
        {
            maps.Add(new MapData
            {
                mapName = "First Map",
                mapPrefab = PrefabDatabase.Instance.firstMapPrefab,
                icon = null // Có thể thêm field icon vào PrefabDatabase
            });
        }

        if (PrefabDatabase.Instance.secondMapPrefab != null)
        {
            maps.Add(new MapData
            {
                mapName = "Second Map",
                mapPrefab = PrefabDatabase.Instance.secondMapPrefab,
                icon = null
            });
        }

        if (PrefabDatabase.Instance.thirdMapPrefab != null)
        {
            maps.Add(new MapData
            {
                mapName = "Third Map",
                mapPrefab = PrefabDatabase.Instance.thirdMapPrefab,
                icon = null
            });
        }

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
        // Reset tất cả buttons
        for (int i = 0; i < mapButtons.Count; i++)
        {
            ResetButtonColor(mapButtons[i]);
        }

        // Highlight button được chọn
        if (selectedMapIndex >= 0 && selectedMapIndex < mapButtons.Count)
        {
            HighlightButton(mapButtons[selectedMapIndex]);
        }
    }

    private void HighlightButton(Button btn)
    {
        if (btn == null) return;
        ColorBlock c = btn.colors;
        c.normalColor = Color.green;
        btn.colors = c;
    }

    private void ResetButtonColor(Button btn)
    {
        if (btn == null) return;
        ColorBlock c = btn.colors;
        c.normalColor = Color.white;
        btn.colors = c;
    }

    // ═════════════════════════════════════════════
    // START GAME
    // ═════════════════════════════════════════════

    private void StartGame()
    {
        if (availableCharacters.Count == 0)
        {
            Debug.LogError("[ReadyScene] Chua co character!");
            return;
        }

        if (PrefabDatabase.Instance != null)
        {
            PrefabDatabase.Instance.SetSelectedCharacter(availableCharacters[currentCharacterIndex]);
            PrefabDatabase.Instance.SetSelectedMap(selectedMapIndex);
            
            Debug.Log($"[ReadyScene] Start - Character: {availableCharacters[currentCharacterIndex].displayName}, Map: {selectedMapIndex}");
            
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("[ReadyScene] PrefabDatabase not found!");
        }
    }

    // ═════════════════════════════════════════════
    // CLEANUP
    // ═════════════════════════════════════════════

    private void OnDestroy()
    {
        if (btnPreviousCharacter != null) btnPreviousCharacter.onClick.RemoveAllListeners();
        if (btnNextCharacter != null)     btnNextCharacter.onClick.RemoveAllListeners();
        if (btnNext != null)              btnNext.onClick.RemoveAllListeners();
        if (btnBack != null)              btnBack.onClick.RemoveAllListeners();
        if (btnStartGame != null)         btnStartGame.onClick.RemoveAllListeners();

        foreach (Button btn in mapButtons)
        {
            if (btn != null)
                btn.onClick.RemoveAllListeners();
        }

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
    public string mapName;
    public GameObject mapPrefab;
    public Sprite icon; // Icon hiển thị trên button
}