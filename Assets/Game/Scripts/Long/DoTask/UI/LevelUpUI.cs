using UnityEngine;
using System.Collections.Generic;

public class LevelUpUI : MonoBehaviour
{
    public GameObject panel;
    public LevelUpOptionUI optionPrefab;
    public Transform optionsParent;

    List<LevelUpOptionUI> spawned = new();

    private void OnEnable()
    {
        if (LevelManager.Instance != null)
            Register();
        else
            LevelManager.OnCreated += HandleCreated;
    }

    void HandleCreated(object _, System.EventArgs __)
    {
        LevelManager.OnCreated -= HandleCreated;
        Register();
    }

    void Register()
    {
        LevelManager.Instance.OnLevelUp += OnLevelUp;
        LevelManager.Instance.OnUpgradeApplied += OnUpgradeApplied;
        panel.SetActive(false);
    }

    private void OnDisable()
    {
        if (LevelManager.Instance == null) return;

        LevelManager.Instance.OnLevelUp -= OnLevelUp;
        LevelManager.Instance.OnUpgradeApplied -= OnUpgradeApplied;
    }

    void OnLevelUp(object _, LevelManager.LevelUpEventArgs e)
    {
        panel.SetActive(true);

        foreach (var o in spawned) Destroy(o.gameObject);
        spawned.Clear();

        foreach (var opt in e.Options)
        {
            var ui = Instantiate(optionPrefab, optionsParent);
            ui.Setup(opt, LevelManager.Instance.ApplyUpgrade);
            spawned.Add(ui);
        }
    }

    void OnUpgradeApplied(object _, LevelManager.UpgradeSelectedEventArgs __)
    {
        panel.SetActive(false);
    }
}
