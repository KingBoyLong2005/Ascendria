using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
// using UnityEngine.UIElements;

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

    [Header("Map Selection")]
    [SerializeField] private Button btnFirstMap;
    [SerializeField] private Button btnSecondMap;
    [SerializeField] private Button btnThirdMap;
    [SerializeField] private TMP_Text txtSelectedMap;

    [Header("Game Control")]
    [SerializeField] private Button btnStartGame;
    [SerializeField] private string gameSceneName = "GameScene";

    // Runtime
    private int currentCharacterIndex = 0;
    private int selectedMapIndex = 0;
    private GameObject currentCharacterModel;

    private void Start()
    {
        if (btnPreviousCharacter != null)
            btnPreviousCharacter.onClick.AddListener(PreviousCharacter);

        if (btnNextCharacter != null)
            btnNextCharacter.onClick.AddListener(NextCharacter);

        if (btnFirstMap != null)
            btnFirstMap.onClick.AddListener(() => SelectMap(0));

        if (btnSecondMap != null)
            btnSecondMap.onClick.AddListener(() => SelectMap(1));

        if (btnThirdMap != null)
            btnThirdMap.onClick.AddListener(() => SelectMap(2));

        if (btnStartGame != null)
            btnStartGame.onClick.AddListener(StartGame);

        if (availableCharacters.Count > 0)
            DisplayCharacter(0);
        else
            Debug.LogError("[ReadyScene] Chua add ProfileCharacterData vao danh sach!");

        SelectMap(0);
    }

    // ─────────────────────────────────────────────
    // CHARACTER NAVIGATION
    // ─────────────────────────────────────────────

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

    // ─────────────────────────────────────────────
    // DISPLAY CHARACTER
    // ─────────────────────────────────────────────

    private void DisplayCharacter(int index)
    {
        if (index < 0 || index >= availableCharacters.Count) return;

        ProfileCharacterData character = availableCharacters[index];

        // Update stats UI
        if (txtCharacterName != null) txtCharacterName.text = character.displayName;
        if (txtMaxHP != null)         txtMaxHP.text = character.maxHP.ToString("F0");
        if (txtAttack != null)        txtAttack.text = character.CharacterAttack.ToString("F1");
        if (txtArmor != null)         txtArmor.text = character.Armor.ToString("F1");
        if (txtMoveSpeed != null)     txtMoveSpeed.text = character.MoveSpeed.ToString("F1");
        if (txtLuck != null)          txtLuck.text = character.Luck.ToString("F1");
        if (txtWealth != null)        txtWealth.text = character.Wealth.ToString("F1");
        if (txtWise != null)          txtWise.text = character.Wise.ToString("F1");

        // Spawn model 3D
        SpawnPreviewModel(character);
    }

    private void SpawnPreviewModel(ProfileCharacterData character)
    {
        // Xoa model cu
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

        // Spawn thang vao World Space tai vi tri characterPreviewRoot
        // KHONG lam child cua Canvas → model hien thi 3D binh thuong
        currentCharacterModel = Instantiate(
            character.modelPrefab,
            characterPreviewRoot.position,
            characterPreviewRoot.rotation
        );
        currentCharacterModel.transform.localScale = Vector3.one * 100;
        currentCharacterModel.transform.rotation = Quaternion.Euler( new Vector3(0,-180,0));
        // Play idle animation
        Animator animator = currentCharacterModel.GetComponentInChildren<Animator>();
        if (animator != null && character.animatorController != null)
        {
            animator.runtimeAnimatorController = character.animatorController;
            animator.SetBool("IsGrounded", true);
        }
    }

    // ─────────────────────────────────────────────
    // MAP SELECTION
    // ─────────────────────────────────────────────

    private void SelectMap(int mapIndex)
    {
        selectedMapIndex = mapIndex;

        if (txtSelectedMap != null)
        {
            string[] mapNames = { "First Map", "Second Map", "Third Map" };
            if (mapIndex >= 0 && mapIndex < mapNames.Length)
                txtSelectedMap.text = "Selected: " + mapNames[mapIndex];
        }

        UpdateMapButtonVisuals();
    }

    private void UpdateMapButtonVisuals()
    {
        ResetButtonColor(btnFirstMap);
        ResetButtonColor(btnSecondMap);
        ResetButtonColor(btnThirdMap);

        switch (selectedMapIndex)
        {
            case 0: HighlightButton(btnFirstMap);  break;
            case 1: HighlightButton(btnSecondMap); break;
            case 2: HighlightButton(btnThirdMap);  break;
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

    // ─────────────────────────────────────────────
    // START GAME
    // ─────────────────────────────────────────────

    private void StartGame()
    {
        if (availableCharacters.Count == 0)
        {
            Debug.LogError("[ReadyScene] Chua co character nao!");
            return;
        }

        if (PrefabDatabase.Instance != null)
        {
            PrefabDatabase.Instance.SetSelectedCharacter(availableCharacters[currentCharacterIndex]);
            PrefabDatabase.Instance.SetSelectedMap(selectedMapIndex);
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("[ReadyScene] PrefabDatabase not found!");
        }
    }

    // ─────────────────────────────────────────────
    // CLEANUP
    // ─────────────────────────────────────────────

    private void OnDestroy()
    {
        if (btnPreviousCharacter != null) btnPreviousCharacter.onClick.RemoveAllListeners();
        if (btnNextCharacter != null)     btnNextCharacter.onClick.RemoveAllListeners();
        if (btnFirstMap != null)          btnFirstMap.onClick.RemoveAllListeners();
        if (btnSecondMap != null)         btnSecondMap.onClick.RemoveAllListeners();
        if (btnThirdMap != null)          btnThirdMap.onClick.RemoveAllListeners();
        if (btnStartGame != null)         btnStartGame.onClick.RemoveAllListeners();

        if (currentCharacterModel != null)
            Destroy(currentCharacterModel);
    }
}