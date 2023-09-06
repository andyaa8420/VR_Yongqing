using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XRSpace.Platform.IME
{
	public class XRIMETMPInputfieldCursorAgent : XRIMEKeyBoardCursorAgent, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
	{
		#region private-field
		private TMP_InputField _inputField;

		private int? _valueChangeCursor;
		private Coroutine _selectJob = null;

		private XRIMEFlow _flow;
		private XRIMEStateTransitionInfo _info;
		private XRIMEState _showState;
		private XRIMEState _keyinState;
		private XRIMEState _keyinHandleState;

		private bool _isEventAdded = false;
		private bool _isEnterShow = false;

		private int _dragCursor;
		private Action<PointerEventData> _selectAllEvent;
		private int _exceptedSuggestionLengthChange = 0;
		private int _exceptedSuggestionLength = 0;
		private bool _isSuggestion = false;

		//private bool _isCanHandleKeyin = false;
		#endregion private-field

		#region public-method
		public override void SetCursor(int caretPos)
		{
			//Debug.LogFormat("[XRIMETMPInputfieldCursorAgent.SetInputfieldCursor] set caretPos {0}", caretPos);

			CaretPos = _inputField.caretPosition = caretPos;
			SelectionAnchorPos = _inputField.selectionAnchorPosition = caretPos;
			SelectionFocusPos = _inputField.selectionFocusPosition = caretPos;
		}
		
		public override void OverwriteCursor(int caretPos)
		{
			_valueChangeCursor = caretPos;
		}

		public override void SendKeyEvent(KeyCode keyCode)
		{
			//if (Application.isEditor)
			//{
			//	Event keyEvent = new Event();
			//	keyEvent.keyCode = keyCode;
			//	_inputField.ProcessEvent(keyEvent);
			//	keyEvent.Use();
			//}
		}

		public override void InsertText(int startPos, int length, string text)
		{
			//if (Application.isEditor)
			//{
			//	var result = _inputField.text;

			//	result = result.Remove(startPos, length);
			//	result = result.Insert(startPos, text);
			//	_inputField.text = result;
			//}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (_inputField != null && !_inputField.shouldHideMobileInput)
			{
				Debug.LogWarningFormat("[XRIMETMPInputfieldCursorAgent.OnPointerDown] shouldHideMobileInput is false. \"{0}\"", _inputField.gameObject);
			}

			if (IsCaretInfoChange())
			{
				XRIMECursorSuggestion.ClearText();
				_exceptedSuggestionLength = 0;
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (_inputField != null && !_inputField.shouldHideMobileInput)
			{
				Debug.LogWarningFormat("[XRIMETMPInputfieldCursorAgent.OnPointerClick] shouldHideMobileInput is false. \"{0}\"", _inputField.gameObject);
			}

			if (_selectAllEvent != null)
			{
				_selectAllEvent(eventData);
			}
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			_dragCursor = _inputField.caretPosition;

			_selectAllEvent = null;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (_dragCursor != _inputField.caretPosition)
			{
				XRIMECursorSuggestion.ClearText();
				_exceptedSuggestionLength = 0;
			}
		}
		#endregion public-method

		#region MonoBehaviour-method
		private void Awake()
		{
			GetSource();
			BuildFlow();
			enabled = false;
		}

		private void OnEnable()
		{
			Debug.LogFormat("[XRIMETMPInputfieldCursorAgent.OnEnable]");
			AddEvents();
			_flow.Start();
			XRIMECursorSuggestion.ClearText();
			_exceptedSuggestionLength = 0;
		}

		private void OnDisable()
		{
			Debug.LogFormat("[XRIMETMPInputfieldCursorAgent.OnDisable]");
			RemoveEvents();
			_flow.Stop();

			if (_inputField != null)
			{
				_inputField.DeactivateInputField();
			}
		}
		
		private void LateUpdate()
		{
			if (_flow != null)
			{
				_flow.UpdateFlow(Time.deltaTime);
			}

			if (_inputField == null)
			{
				throw new Exception("");
			}

			if (_inputField.isFocused)
			{
				SelectionAnchorPos = _inputField.selectionAnchorPosition;
				SelectionFocusPos = _inputField.selectionFocusPosition;
				CaretPos = _inputField.caretPosition;
				Edited = false;
			}
		}

		private void OnDestroy()
		{
			RemoveSoure();
		}
		#endregion MonoBehaviour-method

		#region private-method
		private void AddEvents()
		{
			if (_isEventAdded)
			{
				return;
			}
			XRIMEjni.ImsBackCursorCBF += SetCursor;
			_isEventAdded = true;
		}

		private void RemoveEvents()
		{
			if (!_isEventAdded)
			{
				return;
			}
			XRIMEjni.ImsBackCursorCBF -= SetCursor;
			_isEventAdded = false;
		}

		private void BuildFlow()
		{
			_info = new XRIMEStateTransitionInfo();
			_flow = new XRIMEFlow(_info);

			_showState = XRIMEState.Create(_flow, "TMPShowState");
			_showState.OnEnterStateEvent += OnEnterShow;
			_showState.OnExitStateEvent += OnExitShow;

			_keyinState = XRIMEState.Create(_flow, "TMPKeyinState");
			_keyinState.OnEnterStateEvent += OnEnterKeyin;

			_keyinHandleState = XRIMEState.Create(_flow, "TMPKeyinHandleState");
			_keyinHandleState.OnEnterStateEvent += OnEnterKeyinhandle;

			_showState.MakeConnection(_keyinState, IsKeyin);
			_showState.MakeConnection(_showState, IsReopen);

			_keyinState.MakeConnection(_keyinHandleState, IsCanHandleKeyEvent);

			_keyinHandleState.MakeConnection(_showState, IsAlwaysTrue);

			_flow.SetRootState(_showState);
			_flow.SetCurrentState(_showState);
			_flow.Start();
		}

		private void GetSource()
		{
			_inputField = GetComponent<TMP_InputField>();
			if (_inputField == null)
			{
				throw new Exception("");
			}
			Debug.LogFormat("[XRIMETMPInputfieldCursorAgent.GetSource] _inputField : {0}", _inputField.name);

			_inputField.shouldHideMobileInput = true;
			_inputField.onValueChanged.AddListener(OnValueChange);
		}

		private void RemoveSoure()
		{
			if (_inputField == null)
			{
				return;
			}

			_inputField.onValueChanged.RemoveListener(OnValueChange);
		}

		private IEnumerator SelectInputfieldJob(TMP_InputField inputfield)
		{
			while (!Application.isFocused)
			{
				yield return null;
			}
			var caretPos = CaretPos;
			var selectionAnchorPos = SelectionAnchorPos;
			var selectionFocusPos = SelectionFocusPos;
			//Debug.LogFormat("[XRIMETMPInputfieldCursorAgent.SelectInputfieldJob] {0}", caretPos);

			inputfield.Select();
			inputfield.caretBlinkRate = 0.01f;
			var tempColor = inputfield.selectionColor;
			inputfield.selectionColor = Color.clear;

			while (!inputfield.isFocused)
			{
				yield return null;
			}
			//Debug.LogFormat("[XRIMETMPInputfieldCursorAgent.SelectInputfieldJob] isFocused caretPos {0}", caretPos);

			inputfield.caretPosition = caretPos;
			inputfield.selectionAnchorPosition = selectionAnchorPos;
			inputfield.selectionFocusPosition = selectionFocusPos;
			inputfield.ForceLabelUpdate();
			inputfield.selectionColor = tempColor;

			_selectJob = null;
		}

		private void OnValueChange(string text)
		{
			//Debug.LogFormat("[XRIMETMPInputfieldCursorAgent.OnValueChange] text : \"{0}\"", text);

			if (_valueChangeCursor == null)
			{
				return;
			}
			var pos = _valueChangeCursor.Value;
			//Debug.LogFormat("[XRIMETMPInputfieldCursorAgent.OnValueChange] set caretPos {0}", pos);
			SetCursor(pos);
			_valueChangeCursor = null;
			_inputField.caretPosition = pos;
			_inputField.selectionAnchorPosition = pos;
			_inputField.selectionFocusPosition = pos;

			if (_isSuggestion)
			{
				//Debug.LogFormat("[XRIMETMPInputfieldCursorAgent.OnValueChange] _isSuggestion");
				_isSuggestion = false;

				if (_exceptedSuggestionLength > 0 && pos - _exceptedSuggestionLength >= 0)
				{
					var subStr = text.Substring(pos - _exceptedSuggestionLength, _exceptedSuggestionLength);
					//Debug.LogFormat("[XRIMETMPInputfieldCursorAgent.OnValueChange] subStr \"{0}\"", subStr);
					XRIMEEventSystem.ApplySuggestion(subStr);
				}

				_exceptedSuggestionLength = 0;
			}
			else
			{
				//Debug.LogFormat("[XRIMETMPInputfieldCursorAgent.OnValueChange] {0}, {1}", _exceptedSuggestionLength, _exceptedSuggestionLengthChange);

				_exceptedSuggestionLength = _exceptedSuggestionLength + _exceptedSuggestionLengthChange;
				_exceptedSuggestionLength = Math.Max(_exceptedSuggestionLength, 0);
				_exceptedSuggestionLengthChange = 0;

				if (_exceptedSuggestionLength > 0 && pos - _exceptedSuggestionLength >= 0)
				{
					var subStr = text.Substring(pos - _exceptedSuggestionLength, _exceptedSuggestionLength);
					//Debug.LogFormat("[XRIMETMPInputfieldCursorAgent.OnValueChange] subStr \"{0}\"", subStr);
					XRIMECursorSuggestion.SetText(subStr);
				}
				else
				{
					XRIMECursorSuggestion.SetText(string.Empty);
				}
			}
		}

		private void OnEnterShow(XRIMEState state)
		{
			XRIMEKeyBoardCursorAgentManager.IsLockAgent = false;
			if (_isEnterShow)
			{
				return;
			}
			_isEnterShow = true;
			XRIMEEventSystem.OnKeyCodeTrigger += DoKeyCode;
			XRIMEEventSystem.OnReplaceKeyTrigger += DoReplaceKey;
		}

		private void OnExitShow(XRIMEState state)
		{
			if (!_isEnterShow)
			{
				return;
			}
			_isEnterShow = false;
			XRIMEEventSystem.OnKeyCodeTrigger -= DoKeyCode;
			XRIMEEventSystem.OnReplaceKeyTrigger -= DoReplaceKey;
		}

		private void OnEnterKeyin(XRIMEState state)
		{
			if (_selectJob == null)
			{
				_selectJob = StartCoroutine(SelectInputfieldJob(_inputField));
			}
		}

		private void DoKeyCode(string keyCode)
		{
			//Debug.LogFormat("[XRIMETMPInputfieldCursorAgent.DoKeyCode] \"{0}\"", keyCode);
			_info.IsClickKey = true;

			switch (keyCode)
			{
				case XRIMEEventSystem.KeyCodeEnter:
					{
						XRIMEjni.imsSendKeyEvent((int)KEY_EVENT.KEY_ENTER);
						XRIMECursorSuggestion.ClearText();
						_exceptedSuggestionLength = 0;
						_info.IsClickKey = false;
					}
					break;
				case XRIMEEventSystem.KeyCodeDelete:
					{
						//Debug.Log($"[XRIMETMPInputfieldCursorAgent.DoKeyCode] Delete");

						_exceptedSuggestionLengthChange = -1;
						var deleteLen = 0;
						var cursorPos = 0;
						GetSelectionData(out deleteLen, ref cursorPos);

						XRIMEjni.imsDeleteTextEvent(cursorPos + deleteLen, Math.Max(deleteLen, 1));

						var cursorFix = deleteLen == 0 ? -1 : 0;
						OverwriteCursor(cursorPos + cursorFix);
						XRIMEKeyBoardCursorAgentManager.IsLockAgent = true;
					}
					break;
				case XRIMEEventSystem.KeyCodeClose:
					{
						XRIMECursorSuggestion.ClearText();
						_exceptedSuggestionLength = 0;
						_info.IsClickKey = false;
					}
					break;
				case XRIMEEventSystem.KeyCodeLang:
					{
						XRIMECursorSuggestion.ClearText();
						_exceptedSuggestionLength = 0;
						XRIMEKeyBoardCursorAgentManager.IsLockAgent = true;
						InputEmpty();
					}
					break;
				case XRIMEEventSystem.KeyCodePunc:
				case XRIMEEventSystem.KeyCodeShift:
					{
						XRIMEKeyBoardCursorAgentManager.IsLockAgent = true;
						InputEmpty();
					}
					break;
				default:
					{
						if (keyCode == XRIMEEventSystem.KeyCodeCom)
						{
							keyCode = ComText;
						}
						//Debug.Log($"[XRIMETMPInputfieldCursorAgent.DoKeyCode] KeyCode : \"{keyCode}\"");

						if (!string.IsNullOrEmpty(keyCode) && (!char.IsLetter(keyCode[0]) && !char.IsNumber(keyCode[0]) && !char.IsWhiteSpace(keyCode[0])))
						{
							XRIMECursorSuggestion.ClearText();
							_exceptedSuggestionLength = 0;
						}
						else
						{
							_exceptedSuggestionLengthChange = keyCode.Length;
						}

						var selectionLen = 0;
						var cursorPos = 0;
						GetSelectionData(out selectionLen, ref cursorPos);

						if (selectionLen > 0)
						{
							XRIMEjni.imsSetComposingRegionEvent(cursorPos, cursorPos + selectionLen);
							XRIMEjni.imsSetComposingTextEvent(keyCode);
						}
						else
						{
							XRIMEjni.imsCommitTextEvent(keyCode, cursorPos);
						}

						XRIMEKeyBoardCursorAgentManager.IsLockAgent = true;
						OverwriteCursor(cursorPos + keyCode.Length);
					}
					break;
			}
		}

		private void DoReplaceKey(int length, string keyCode)
		{
			_isSuggestion = true;
			_exceptedSuggestionLength = keyCode.Length;

			_info.IsClickKey = true;

			//Debug.Log($"[XRIMEInputfieldCursorAgent.DoReplaceKey] length : {length} KeyCode : \"{keyCode}\"");
			var deleteLen = 0;
			var cursorPos = 0;
			GetSelectionData(out deleteLen, ref cursorPos);

			if (length > 0)
			{
				XRIMEjni.imsSetComposingRegionEvent(cursorPos - length, cursorPos - length + length);
				XRIMEjni.imsSetComposingTextEvent(keyCode);
			}
			else
			{
				XRIMEjni.imsCommitTextEvent(keyCode, cursorPos - length);
			}

			XRIMEKeyBoardCursorAgentManager.IsLockAgent = true;
			OverwriteCursor(cursorPos - length + keyCode.Length);
		}

		private void OnEnterKeyinhandle(XRIMEState state)
		{
			//if (_keyinHandleEvents != null)
			//{
			//	_keyinHandleEvents();
			//	_keyinHandleEvents = null;
			//}
		}

		public bool IsKeyin(XRIMEStateTransitionInfo info)
		{
			var result = info.IsClickKey;
			if (result)
			{
				Debug.Log("[XRIMETMPInputfieldCursorAgent.IsKeyin] IsClickKey");
			}
			return result;
		}

		public bool IsReopen(XRIMEStateTransitionInfo info)
		{
			return info.HasState(IME_StatusEvent.ShowWindow);
		}

		private bool IsCanHandleKeyEvent(XRIMEStateTransitionInfo info)
		{
			return info.HasState(IME_StatusEvent.ShowWindow);
		}

		private bool IsAlwaysTrue(XRIMEStateTransitionInfo info)
		{
			return true;
		}

		private void GetSelectionData(out int selectionLength, ref int cursorPos)
		{
			selectionLength = 0;
			if (!Edited)
			{
				cursorPos = Math.Min(SelectionAnchorPos, SelectionFocusPos);
				selectionLength = Math.Abs(SelectionAnchorPos - SelectionFocusPos);

				Edited = true;
			}
		}

		private bool IsCaretInfoChange()
		{
			var isDiff = false;
			isDiff = SelectionAnchorPos != _inputField.selectionAnchorPosition ||
				SelectionFocusPos != _inputField.selectionFocusPosition ||
				CaretPos != _inputField.caretPosition;
	
			return isDiff;
		}

		/// <summary>
		/// For solving tmp shift/punkey/langkey select all;
		/// </summary>
		private void InputEmpty()
		{
			var selectionLen = 0;
			var cursorPos = 0;
			GetSelectionData(out selectionLen, ref cursorPos);
			if (selectionLen == 0)
			{
				XRIMEjni.imsCommitTextEvent(string.Empty, cursorPos);
			}
		}
		#endregion private-method
	}
}