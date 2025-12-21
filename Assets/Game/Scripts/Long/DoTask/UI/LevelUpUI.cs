using System.Collections.Generic;
using UnityEngine;

public class LevelUpUI : MonoBehaviour
{
    public GameObject panel;
    public LevelUpOptionUI optionPrefab;
    public Transform optionsParent;

    List<LevelUpOptionUI> spawned = new();

    public void Show(LevelManager.LevelUpEventArgs e)
    {
        GameManager.Instance.PauseGame();
        panel.SetActive(true);
        FindFirstObjectByType<TPCameraController>().isUIOpen = true;

        foreach (var o in spawned) Destroy(o.gameObject);
        spawned.Clear();

        foreach (var opt in e.Options)
        {
            var ui = Instantiate(optionPrefab, optionsParent);
            ui.Setup(opt, LevelManager.Instance.ApplyUpgrade);
            spawned.Add(ui);
        }
    }

    public void Hide()
    {
        GameManager.Instance.ResumeGame();
        panel.SetActive(false);
        FindFirstObjectByType<TPCameraController>().isUIOpen = false;
    }
}

// using UnityEngine;
// using System.Collections.Generic;

// public class LevelUpUI : MonoBehaviour
// {
//     public GameObject panel;
//     public LevelUpOptionUI optionPrefab;
//     public Transform optionsParent;

//     List<LevelUpOptionUI> spawned = new();

//     // private void OnEnable()
//     // {
//     //     if (LevelManager.Instance != null)
//     //         Register();
//     //     else
//     //         LevelManager.OnCreated += HandleCreated;
//     // }

//     // void HandleCreated(object _, System.EventArgs __)
//     // {
//     //     LevelManager.OnCreated -= HandleCreated;
//     //     Register();
//     // }

//     // public void Register()
//     // {
//     //     LevelManager.Instance.OnLevelUp += OnLevelUp;
//     //     LevelManager.Instance.OnUpgradeApplied += OnUpgradeApplied;
//     //     panel.SetActive(false);
//     // }

//     // private void OnDisable()
//     // {
//     //     if (LevelManager.Instance == null) return;

//     //     LevelManager.Instance.OnLevelUp -= OnLevelUp;
//     //     LevelManager.Instance.OnUpgradeApplied -= OnUpgradeApplied;
//     // }

//     public void OnLevelUp(object _, LevelManager.LevelUpEventArgs e)
//     {
//         panel.SetActive(true);
//         FindFirstObjectByType<TPCameraController>().isUIOpen = true;
//         foreach (var o in spawned) Destroy(o.gameObject);
//         spawned.Clear();

//         foreach (var opt in e.Options)
//         {
//             var ui = Instantiate(optionPrefab, optionsParent);
//             ui.Setup(opt, LevelManager.Instance.ApplyUpgrade);
//             spawned.Add(ui);
//         }
//     }

//     public void OnUpgradeApplied(object _, LevelManager.UpgradeSelectedEventArgs __)
//     {
//         panel.SetActive(false);
//         FindFirstObjectByType<TPCameraController>().isUIOpen = false;
//     }
// }
