using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RestartButton : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI _text;
    private void Start() {
        _text.enabled = false;
        enabled = false;
    }

    public void ActivateRestartButton() {
        _text.enabled = true;
        enabled = true;
    }
    private void OnMouseDown()
    {
        if (enabled) {
            GameManager gameManager = FindObjectOfType<GameManager>();
            int[] nextLevel = CalculateNextLevel(gameManager);
            gameManager.ResetBoard();
            gameManager.GenerateGrid(nextLevel);
            _text.enabled = false;
            enabled = false;
        }
    }

    private int[] CalculateNextLevel(GameManager gameManager)
    {
        int[] currentLevel = gameManager.GetCurrentLevel();
        if (currentLevel == Constants.numbersForLvl1) {
            return Constants.numbersForLvl2;
        }
        if (currentLevel == Constants.numbersForLvl2) {
            return Constants.numbersForLvl3;
        }
        if (currentLevel == Constants.numbersForLvl3) {
            return Constants.numbersForLvl4;
        }
        if (currentLevel == Constants.numbersForLvl4) {
            return Constants.numbersForLvl5;
        }
        if (currentLevel == Constants.numbersForLvl5) {
            return Constants.numbersForLvl6;
        }
        if (currentLevel == Constants.numbersForLvl6) {
            return Constants.numbersForLvl7;
        }
        if (currentLevel == Constants.numbersForLvl7) {
            return Constants.numbersForLvl8;
        }
        return Constants.numbersForLvl1;
    }
}