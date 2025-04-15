using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using CandyCabinets.Components.Colour;
using Assets.Scripts.CustomAnimation;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine.UI;
using System;

public class Block : MonoBehaviour
{
    public int Value;

    [SerializeField]
    private TextMeshPro _text;

    [SerializeField]
    private Image _result;
    private GameManager _gameManager;
    private UIManager _uiManager;
    private Node _originalNode;
    private bool _isInteractible = false;
    public bool IsSelected = false;
    float dragTime = 0f;

    [SerializeField]
    private Sprite _correctResultSprite;

    [SerializeField]
    private Sprite _incorrectResultSprite;

    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _uiManager = FindObjectOfType<UIManager>();
    }

    public Node GetNode()
    {
        return _originalNode;
    }

    public Block Init(int value, bool interactible, Node node)
    {
        Value = value;
        _text.text = value.ToString();
        _isInteractible = interactible;
        gameObject.name = string.Concat("Block_", value.ToString());
        if (!interactible)
        {
            _text.fontSizeMax = 60f;
        }
        _originalNode = node;
        transform.SetParent(node.transform, false);
        _text.color = interactible
            ? ColourManager.Instance.SelectedPalette().Colours[Constants.COLOR_DARK_TEXT]
            : ColourManager.Instance.SelectedPalette().Colours[Constants.COLOR_LIGHT_TEXT];
        return this;
    }

    private async void OnMouseDown()
    {
        Node nodeClickedOn = GetNodeTouched();
        if (nodeClickedOn != null && _isInteractible)
        {
            if (_gameManager.SavedGameData.SettingsData.ControlMethodDrag)
            {
                CustomAnimation.NumberClicked(transform);
                _ = CustomAnimation.NodeClicked(nodeClickedOn.transform);
                _uiManager.InteractionPerformed(Constants.AudioClip.DropBlock);
            }
            else
            {
                Block selectedBlock = FindObjectOfType<GameManager>().GetSelectedBlock();
                if (selectedBlock == null)
                {
                    FindObjectOfType<GameManager>().ResetSelectedBlock();
                    IsSelected = true;
                    nodeClickedOn.UpdateColor(
                        ColourManager.Instance.SelectedPalette().Colours[
                            Constants.COLOR_SELECTED_NODE
                        ]
                    );
                    _uiManager.InteractionPerformed(Constants.AudioClip.DropBlock);
                    await CustomAnimation.NodeClicked(nodeClickedOn.transform);
                }
                else if (selectedBlock._originalNode.name == _originalNode.name)
                {
                    FindObjectOfType<GameManager>().ResetSelectedBlock();
                    selectedBlock._originalNode.UpdateColor(
                        ColourManager.Instance.SelectedPalette().Colours[
                            Constants.COLOR_NODE_NEUTRAL
                        ]
                    );
                    _uiManager.InteractionPerformed(Constants.AudioClip.DropBlockUndo);
                    await CustomAnimation.NodeClicked(selectedBlock._originalNode.transform);
                }
                else if (selectedBlock._originalNode.name != _originalNode.name)
                {
                    if (_originalNode != null && _originalNode.name != selectedBlock.GetNode().name)
                    {
                        _originalNode.UpdateColor(
                            ColourManager.Instance.SelectedPalette().Colours[
                                Constants.COLOR_SELECTED_NODE
                            ]
                        );

                        selectedBlock._originalNode.UpdateColor(
                            ColourManager.Instance.SelectedPalette().Colours[
                                Constants.COLOR_NODE_NEUTRAL
                            ]
                        );
                        _originalNode.UpdateColor(
                            ColourManager.Instance.SelectedPalette().Colours[
                                Constants.COLOR_NODE_NEUTRAL
                            ]
                        );
                        _gameManager.StoreUndoData(selectedBlock._originalNode, _originalNode);
                        _ = CustomAnimation.NodeClicked(selectedBlock._originalNode.transform);
                        _ = CustomAnimation.NodeClicked(nodeClickedOn.transform);
                        _uiManager.InteractionPerformed(Constants.AudioClip.DropBlock);
                        await SwitchBlocksUndo(nodeClickedOn, selectedBlock._originalNode);

                        FindObjectOfType<GameManager>().ResetSelectedBlock();
                        if (!FindObjectOfType<GameManager>().CheckResult(true))
                        {
                            _gameManager.ShowOnboardingClassicUndo();
                        }
                    }
                }
            }
            FindObjectOfType<GameManager>().RemoveHints();
        }
        else if (
            nodeClickedOn != null
            && !_isInteractible
            && !_gameManager.SavedGameData.SettingsData.ControlMethodDrag
        )
        {
            _uiManager.InteractionPerformed(Constants.AudioClip.DropBlockUndo);
        }
    }

    private Vector3 MoveOffsetPosition()
    {
        Vector3 originalPosition = GetWorldMousePosition();
        originalPosition.z = 0;
        return originalPosition;
    }

    private void OnMouseDrag()
    {
        if (_gameManager.SavedGameData.SettingsData.ControlMethodDrag && _isInteractible)
        {
            Node nodeWhereBlockIsHovering = GetNodeTouched();
            _text.DOFade(0.66f, .25f);
            transform.position = MoveOffsetPosition();
            CustomAnimation.NumberClicked(transform);
            if (nodeWhereBlockIsHovering != null && _originalNode != nodeWhereBlockIsHovering)
            {
                dragTime += Time.deltaTime;
            }
            else
            {
                dragTime = 0f;
            }

            if (
                nodeWhereBlockIsHovering != null
                && _originalNode != nodeWhereBlockIsHovering
                && dragTime > .8f
            )
            {
                _gameManager.StoreUndoData(_originalNode, nodeWhereBlockIsHovering);
                SwitchBlocks(nodeWhereBlockIsHovering);
                _originalNode = nodeWhereBlockIsHovering;
                dragTime = 0f;
            }
        }
    }

    private void OnMouseUp()
    {
        if (
            !_gameManager.HasGameEnded()
            && _gameManager.SavedGameData.SettingsData.ControlMethodDrag
        )
        {
            Node nodeWhereBlockIsDropped = GetNodeTouched();
            if (nodeWhereBlockIsDropped != null && _isInteractible)
            {
                CustomAnimation.NumberDropped(
                    _originalNode.GetBlockInNode().transform,
                    nodeWhereBlockIsDropped.transform.position
                );
                if (_originalNode != nodeWhereBlockIsDropped)
                {
                    _uiManager.InteractionPerformed(Constants.AudioClip.DropBlock);
                    _gameManager.StoreUndoData(_originalNode, nodeWhereBlockIsDropped);
                    SwitchBlocks(nodeWhereBlockIsDropped);
                    _originalNode = nodeWhereBlockIsDropped;
                    _ = CustomAnimation.NodeClicked(nodeWhereBlockIsDropped.transform);
                }
                else
                {
                    _uiManager.InteractionPerformed(Constants.AudioClip.DropBlockUndo);
                    _ = CustomAnimation.NodeClicked(nodeWhereBlockIsDropped.transform);
                }
                if (!FindObjectOfType<GameManager>().CheckResult(true))
                {
                    _gameManager.ShowOnboardingClassicUndo();
                }
            }
            else if (nodeWhereBlockIsDropped == null || !_isInteractible)
            {
                CustomAnimation.NumberDropped(transform, _originalNode.transform.position);
                //if (!FindObjectOfType<GameManager>().CheckResult(true))
                //{
                _uiManager.InteractionPerformed(Constants.AudioClip.DropBlockUndo);
                //}
            }
            _text.DOFade(1f, .25f);
        }
    }

    private Node GetNodeTouched()
    {
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(camRay.origin, camRay.direction, out RaycastHit hitInfo);
        if (
            hitInfo.collider != null
            && hitInfo.collider.GetComponent<Node>() != null
            && hitInfo.collider.GetComponent<Node>().GetBlockInNode()._isInteractible
        )
        {
            return hitInfo.collider.GetComponent<Node>();
        }
        return null;
    }

    private Vector3 GetWorldMousePosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public void ChangeInteraction(bool newStatus)
    {
        _isInteractible = newStatus;
    }

    public bool IsInteractable()
    {
        return _isInteractible;
    }

    public async void SwitchBlocks(Node hoveredNode)
    {
        Node tempNode = hoveredNode;

        hoveredNode.GetBlockInNode().transform.SetParent(_originalNode.transform);
        hoveredNode.GetBlockInNode()._originalNode = _originalNode;
        _originalNode.SetBlockInNode(hoveredNode.GetBlockInNode());
        CustomAnimation.NumberSwitched(
            hoveredNode.GetBlockInNode().transform,
            _originalNode.transform.position
        );

        transform.SetParent(tempNode.transform);
        _originalNode = tempNode;
        hoveredNode.SetBlockInNode(this);

        await CustomAnimation.WaitForAnimation("MoveNumberBack");
    }

    public static async Task<bool> SwitchBlocksUndo(Node secondNode, Node firstNode)
    {
        CustomAnimation.NumberSwitched(
            firstNode.GetBlockInNode().transform,
            secondNode.transform.position
        );
        CustomAnimation.NumberSwitched(
            secondNode.GetBlockInNode().transform,
            firstNode.transform.position
        );

        secondNode.GetBlockInNode().transform.SetParent(firstNode.transform);
        secondNode.GetBlockInNode()._originalNode = firstNode;

        firstNode.GetBlockInNode().transform.SetParent(secondNode.transform);
        firstNode.GetBlockInNode()._originalNode = secondNode;
        Block tempBlock = firstNode.GetBlockInNode();

        firstNode.SetBlockInNode(secondNode.GetBlockInNode());
        secondNode.SetBlockInNode(tempBlock);

        await CustomAnimation.WaitForAnimation("MoveNumberBack");
        return true;
    }

    internal void UpdateTextColor()
    {
        _text.color = _isInteractible
            ? ColourManager.Instance.SelectedPalette().Colours[Constants.COLOR_DARK_TEXT]
            : ColourManager.Instance.SelectedPalette().Colours[Constants.COLOR_LIGHT_TEXT];
        if (_isInteractible)
        {
            HideResultIcon();
        }
        else
        {
            _result.DOColor(
                ColourManager.Instance.SelectedPalette().Colours[Constants.COLOR_LIGHT_TEXT],
                0.1f
            );
        }
    }

    public Sequence AnimatePartialSumCorrect()
    {
        if (_text != null && _text.transform != null)
        {
            return CustomAnimation.SumIsCorrect(_text.transform, GetNode().name);
        }
        return DOTween.Sequence();
        ;
    }

    public Sequence AnimatePuzzleCompleted()
    {
        if (_text != null && _text.transform != null)
        {
            return CustomAnimation.SumIsCorrect(
                GetNode().transform,
                GetNode().transform.position,
                GetNode().name
            );
        }
        return DOTween.Sequence();
        ;
    }

    public async Task AnimateIncorrectSolution()
    {
        await CustomAnimation.SumIsIncorrect(_text.transform, GetNode().name);
    }

    private void OnDestroy()
    {
        DOTween.Kill(this);
        DOTween.Kill(transform);
    }

    internal void UpdateResultIcon(bool showIcon, bool resultIsCorrect)
    {
        if (showIcon)
        {
            ShowResultIcon();
            if (resultIsCorrect)
            {
                _result.sprite = _correctResultSprite;
            }
            else
            {
                _result.sprite = _incorrectResultSprite;
            }
        }
        else
        {
            HideResultIcon();
        }
    }

    internal void ShowResultIcon()
    {
        _result.gameObject.SetActive(true);
    }

    internal void HideResultIcon()
    {
        _result.gameObject.SetActive(false);
    }
}
