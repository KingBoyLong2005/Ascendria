using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class ReadySceneManager : MonoBehaviour
{
    [Header("Character Selection")]
    [SerializeField] private List<ProfileCharacterData> availableCharacters = new List<ProfileCharacterData>();
    [SerializeField] private Transform characterPreviewRoot;
    [SerializeField] private Button btnPreviousCharacter;
    [SerializeField] private Button btnNextCharacter;

    [Header("Character Display UI")]
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
    [SerializeField] private Text txtSelectedMap;

    [Header("Game Control")]
    [SerializeField] private Button btnStartGame;
    [SerializeField] private string gameSceneName = "GameScene";

    // Runtime
    private int currentCharacterIndex = 0;
    private int selectedMapIndex = 0;
    private GameObject currentCharacterModel;

    private void Start()
    {
        // Setup character buttons
        if (btnPreviousCharacter != null)
            btnPreviousCharacter.onClick.AddListener(PreviousCharacter);
        
        if (btnNextCharacter != null)
            btnNextCharacter.onClick.AddListener(NextCharacter);

        // Setup map buttons
        if (btnFirstMap != null)
            btnFirstMap.onClick.AddListener(() => SelectMap(0));
        
        if (btnSecondMap != null)
            btnSecondMap.onClick.AddListener(() => SelectMap(1));
        
        if (btnThirdMap != null)
            btnThirdMap.onClick.AddListener(() => SelectMap(2));

        // Setup start game button
        if (btnStartGame != null)
            btnStartGame.onClick.AddListener(StartGame);

        // Initialize
        if (availableCharacters.Count > 0)
        {
            DisplayCharacter(0);
        }
        else
        {
            Debug.LogError("No characters available! Please add ProfileCharacterData to the list.");
        }

        SelectMap(0); // Default first map
    }

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

    private void DisplayCharacter(int index)
    {
        if (index < 0 || index >= availableCharacters.Count)
            return;

        ProfileCharacterData character = availableCharacters[index];

        // Update character name
        if (txtCharacterName != null)
            txtCharacterName.text = character.displayName;

        // Update stats
        if (txtMaxHP != null)
            txtMaxHP.text = character.maxHP.ToString("F0");
        
        if (txtAttack != null)
            txtAttack.text = character.CharacterAttack.ToString("F1");
        
        if (txtArmor != null)
            txtArmor.text = character.Armor.ToString("F1");
        
        if (txtMoveSpeed != null)
            txtMoveSpeed.text = character.MoveSpeed.ToString("F1");
        
        if (txtLuck != null)
            txtLuck.text = character.Luck.ToString("F1");
        
        if (txtWealth != null)
            txtWealth.text = character.Wealth.ToString("F1");
        
        if (txtWise != null)
            txtWise.text = character.Wise.ToString("F1");

        // Update 3D model preview
        UpdateCharacterModel(character);
    }

    private void UpdateCharacterModel(ProfileCharacterData character)
    {
        // Destroy old model
        if (currentCharacterModel != null)
        {
            Destroy(currentCharacterModel);
        }

        // Create new model
        if (character.modelPrefab != null && characterPreviewRoot != null)
        {
            currentCharacterModel = Instantiate(
                character.modelPrefab,
                characterPreviewRoot.position,
                characterPreviewRoot.rotation,
                characterPreviewRoot
            );

            // Setup animator
            Animator animator = currentCharacterModel.GetComponentInChildren<Animator>();
            if (animator != null && character.animatorController != null)
            {
                animator.runtimeAnimatorController = character.animatorController;
                // Play idle animation
                animator.Play("Idle", 0, 0f);
            }
        }
    }

    private void SelectMap(int mapIndex)
    {
        selectedMapIndex = mapIndex;

        // Update UI to show selected map
        if (txtSelectedMap != null)
        {
            string[] mapNames = { "First Map", "Second Map", "Third Map" };
            if (mapIndex >= 0 && mapIndex < mapNames.Length)
            {
                txtSelectedMap.text = "Selected: " + mapNames[mapIndex];
            }
        }

        // Visual feedback for buttons (optional)
        UpdateMapButtonVisuals();
    }

    private void UpdateMapButtonVisuals()
    {
        // Reset all buttons
        ResetButtonColor(btnFirstMap);
        ResetButtonColor(btnSecondMap);
        ResetButtonColor(btnThirdMap);

        // Highlight selected button
        switch (selectedMapIndex)
        {
            case 0:
                HighlightButton(btnFirstMap);
                break;
            case 1:
                HighlightButton(btnSecondMap);
                break;
            case 2:
                HighlightButton(btnThirdMap);
                break;
        }
    }

    private void HighlightButton(Button btn)
    {
        if (btn != null)
        {
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.green;
            btn.colors = colors;
        }
    }

    private void ResetButtonColor(Button btn)
    {
        if (btn != null)
        {
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            btn.colors = colors;
        }
    }

    private void StartGame()
    {
        if (availableCharacters.Count == 0)
        {
            Debug.LogError("No character selected!");
            return;
        }

        // Save selections to PrefabDatabase
        if (PrefabDatabase.Instance != null)
        {
            PrefabDatabase.Instance.SetSelectedCharacter(availableCharacters[currentCharacterIndex]);
            PrefabDatabase.Instance.SetSelectedMap(selectedMapIndex);

            Debug.Log($"Starting game with character: {availableCharacters[currentCharacterIndex].displayName}");
            Debug.Log($"Map index: {selectedMapIndex}");

            // Load game scene
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("PrefabDatabase not found!");
        }
    }

    private void OnDestroy()
    {
        // Cleanup
        if (btnPreviousCharacter != null)
            btnPreviousCharacter.onClick.RemoveAllListeners();
        
        if (btnNextCharacter != null)
            btnNextCharacter.onClick.RemoveAllListeners();
        
        if (btnFirstMap != null)
            btnFirstMap.onClick.RemoveAllListeners();
        
        if (btnSecondMap != null)
            btnSecondMap.onClick.RemoveAllListeners();
        
        if (btnThirdMap != null)
            btnThirdMap.onClick.RemoveAllListeners();
        
        if (btnStartGame != null)
            btnStartGame.onClick.RemoveAllListeners();
    }
}