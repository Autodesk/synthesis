using UnityEngine;
using System;
using System.Collections.Generic;



#region KeyCode conversions
namespace Internal
{
	/// <summary>
	/// Key code conversions.
	/// </summary>
	static class KeyCodeConversions
	{
		/// <summary>
		/// Map of string conversions for KeyCode.
		/// </summary>
		private static readonly Dictionary<KeyCode, string> toStringMap = new Dictionary<KeyCode, string>()
		{
			  { KeyCode.None,              "None"                 }
			, { KeyCode.Backspace,         "Backspace"            }
			, { KeyCode.Delete,            "Del"                  }
			, { KeyCode.Tab,               "Tab"                  }
			, { KeyCode.Clear,             "Clear"                }
			, { KeyCode.Return,            "Return"               }
			, { KeyCode.Pause,             "Pause"                }
			, { KeyCode.Escape,            "Esc"                  }
			, { KeyCode.Space,             "Space"                }
			, { KeyCode.Keypad0,           "Num 0"                }
			, { KeyCode.Keypad1,           "Num 1"                }
			, { KeyCode.Keypad2,           "Num 2"                }
			, { KeyCode.Keypad3,           "Num 3"                }
			, { KeyCode.Keypad4,           "Num 4"                }
			, { KeyCode.Keypad5,           "Num 5"                }
			, { KeyCode.Keypad6,           "Num 6"                }
			, { KeyCode.Keypad7,           "Num 7"                }
			, { KeyCode.Keypad8,           "Num 8"                }
			, { KeyCode.Keypad9,           "Num 9"                }
			, { KeyCode.KeypadPeriod,      "Num ."                }
			, { KeyCode.KeypadDivide,      "Num /"                }
			, { KeyCode.KeypadMultiply,    "Num *"                }
			, { KeyCode.KeypadMinus,       "Num -"                }
			, { KeyCode.KeypadPlus,        "Num +"                }
			, { KeyCode.KeypadEnter,       "Enter"                }
			, { KeyCode.KeypadEquals,      "Num ="                }
			, { KeyCode.UpArrow,           "Up"                   }
			, { KeyCode.DownArrow,         "Down"                 }
			, { KeyCode.RightArrow,        "Right"                }
			, { KeyCode.LeftArrow,         "Left"                 }
			, { KeyCode.Insert,            "Insert"               }
			, { KeyCode.Home,              "Home"                 }
			, { KeyCode.End,               "End"                  }
			, { KeyCode.PageUp,            "Page Up"              }
			, { KeyCode.PageDown,          "Page Down"            }
			, { KeyCode.F1,                "F1"                   }
			, { KeyCode.F2,                "F2"                   }
			, { KeyCode.F3,                "F3"                   }
			, { KeyCode.F4,                "F4"                   }
			, { KeyCode.F5,                "F5"                   }
			, { KeyCode.F6,                "F6"                   }
			, { KeyCode.F7,                "F7"                   }
			, { KeyCode.F8,                "F8"                   }
			, { KeyCode.F9,                "F9"                   }
			, { KeyCode.F10,               "F10"                  }
			, { KeyCode.F11,               "F11"                  }
			, { KeyCode.F12,               "F12"                  }
			, { KeyCode.F13,               "F13"                  }
			, { KeyCode.F14,               "F14"                  }
			, { KeyCode.F15,               "F15"                  }
			, { KeyCode.Alpha0,            "0"                    }
			, { KeyCode.Alpha1,            "1"                    }
			, { KeyCode.Alpha2,            "2"                    }
			, { KeyCode.Alpha3,            "3"                    }
			, { KeyCode.Alpha4,            "4"                    }
			, { KeyCode.Alpha5,            "5"                    }
			, { KeyCode.Alpha6,            "6"                    }
			, { KeyCode.Alpha7,            "7"                    }
			, { KeyCode.Alpha8,            "8"                    }
			, { KeyCode.Alpha9,            "9"                    }
			, { KeyCode.Exclaim,           "!"                    }
			, { KeyCode.DoubleQuote,       "\""                   }
			, { KeyCode.Hash,              "#"                    }
			, { KeyCode.Dollar,            "$"                    }
			, { KeyCode.Ampersand,         "&"                    }
			, { KeyCode.Quote,             "'"                    }
			, { KeyCode.LeftParen,         "("                    }
			, { KeyCode.RightParen,        ")"                    }
			, { KeyCode.Asterisk,          "*"                    }
			, { KeyCode.Plus,              "+"                    }
			, { KeyCode.Comma,             ","                    }
			, { KeyCode.Minus,             "-"                    }
			, { KeyCode.Period,            "."                    }
			, { KeyCode.Slash,             "/"                    }
			, { KeyCode.Colon,             ":"                    }
			, { KeyCode.Semicolon,         ";"                    }
			, { KeyCode.Less,              "<"                    }
			, { KeyCode.Equals,            "="                    }
			, { KeyCode.Greater,           ">"                    }
			, { KeyCode.Question,          "?"                    }
			, { KeyCode.At,                "@"                    }
			, { KeyCode.LeftBracket,       "["                    }
			, { KeyCode.Backslash,         "\\"                   }
			, { KeyCode.RightBracket,      "]"                    }
			, { KeyCode.Caret,             "^"                    }
			, { KeyCode.Underscore,        "_"                    }
			, { KeyCode.BackQuote,         "`"                    }
			, { KeyCode.A,                 "A"                    }
			, { KeyCode.B,                 "B"                    }
			, { KeyCode.C,                 "C"                    }
			, { KeyCode.D,                 "D"                    }
			, { KeyCode.E,                 "E"                    }
			, { KeyCode.F,                 "F"                    }
			, { KeyCode.G,                 "G"                    }
			, { KeyCode.H,                 "H"                    }
			, { KeyCode.I,                 "I"                    }
			, { KeyCode.J,                 "J"                    }
			, { KeyCode.K,                 "K"                    }
			, { KeyCode.L,                 "L"                    }
			, { KeyCode.M,                 "M"                    }
			, { KeyCode.N,                 "N"                    }
			, { KeyCode.O,                 "O"                    }
			, { KeyCode.P,                 "P"                    }
			, { KeyCode.Q,                 "Q"                    }
			, { KeyCode.R,                 "R"                    }
			, { KeyCode.S,                 "S"                    }
			, { KeyCode.T,                 "T"                    }
			, { KeyCode.U,                 "U"                    }
			, { KeyCode.V,                 "V"                    }
			, { KeyCode.W,                 "W"                    }
			, { KeyCode.X,                 "X"                    }
			, { KeyCode.Y,                 "Y"                    }
			, { KeyCode.Z,                 "Z"                    }
			, { KeyCode.Numlock,           "Num Lock"             }
			, { KeyCode.CapsLock,          "Caps Lock"            }
			, { KeyCode.ScrollLock,        "Scroll Lock"          }
			, { KeyCode.RightShift,        "Right Shift"          }
			, { KeyCode.LeftShift,         "Left Shift"           }
			, { KeyCode.RightControl,      "Right Ctrl"           }
			, { KeyCode.LeftControl,       "Left Ctrl"            }
			, { KeyCode.RightAlt,          "Right Alt"            }
			, { KeyCode.LeftAlt,           "Left Alt"             }
			, { KeyCode.LeftCommand,       "Left Command"         }
//          , { KeyCode.LeftApple,         "Left Apple"           }
			, { KeyCode.LeftWindows,       "Left Windows"         }
			, { KeyCode.RightCommand,      "Right Command"        }
//          , { KeyCode.RightApple,        "Right Apple"          }
			, { KeyCode.RightWindows,      "Right Windows"        }
			, { KeyCode.AltGr,             "AltGr"                }
			, { KeyCode.Help,              "Help"                 }
			, { KeyCode.Print,             "Print"                }
			, { KeyCode.SysReq,            "SysReq"               }
			, { KeyCode.Break,             "Break"                }
			, { KeyCode.Menu,              "Menu"                 }
			, { KeyCode.Mouse0,            "Mouse 0"              }
			, { KeyCode.Mouse1,            "Mouse 1"              }
			, { KeyCode.Mouse2,            "Mouse 2"              }
			, { KeyCode.Mouse3,            "Mouse 3"              }
			, { KeyCode.Mouse4,            "Mouse 4"              }
			, { KeyCode.Mouse5,            "Mouse 5"              }
			, { KeyCode.Mouse6,            "Mouse 6"              }
			, { KeyCode.JoystickButton0,   "Joystick Button 0"    }
			, { KeyCode.JoystickButton1,   "Joystick Button 1"    }
			, { KeyCode.JoystickButton2,   "Joystick Button 2"    }
			, { KeyCode.JoystickButton3,   "Joystick Button 3"    }
			, { KeyCode.JoystickButton4,   "Joystick Button 4"    }
			, { KeyCode.JoystickButton5,   "Joystick Button 5"    }
			, { KeyCode.JoystickButton6,   "Joystick Button 6"    }
			, { KeyCode.JoystickButton7,   "Joystick Button 7"    }
			, { KeyCode.JoystickButton8,   "Joystick Button 8"    }
			, { KeyCode.JoystickButton9,   "Joystick Button 9"    }
			, { KeyCode.JoystickButton10,  "Joystick Button 10"   }
			, { KeyCode.JoystickButton11,  "Joystick Button 11"   }
			, { KeyCode.JoystickButton12,  "Joystick Button 12"   }
			, { KeyCode.JoystickButton13,  "Joystick Button 13"   }
			, { KeyCode.JoystickButton14,  "Joystick Button 14"   }
			, { KeyCode.JoystickButton15,  "Joystick Button 15"   }
			, { KeyCode.JoystickButton16,  "Joystick Button 16"   }
			, { KeyCode.JoystickButton17,  "Joystick Button 17"   }
			, { KeyCode.JoystickButton18,  "Joystick Button 18"   }
			, { KeyCode.JoystickButton19,  "Joystick Button 19"   }
			, { KeyCode.Joystick1Button0,  "Joystick 1 Button 0"  }
			, { KeyCode.Joystick1Button1,  "Joystick 1 Button 1"  }
			, { KeyCode.Joystick1Button2,  "Joystick 1 Button 2"  }
			, { KeyCode.Joystick1Button3,  "Joystick 1 Button 3"  }
			, { KeyCode.Joystick1Button4,  "Joystick 1 Button 4"  }
			, { KeyCode.Joystick1Button5,  "Joystick 1 Button 5"  }
			, { KeyCode.Joystick1Button6,  "Joystick 1 Button 6"  }
			, { KeyCode.Joystick1Button7,  "Joystick 1 Button 7"  }
			, { KeyCode.Joystick1Button8,  "Joystick 1 Button 8"  }
			, { KeyCode.Joystick1Button9,  "Joystick 1 Button 9"  }
			, { KeyCode.Joystick1Button10, "Joystick 1 Button 10" }
			, { KeyCode.Joystick1Button11, "Joystick 1 Button 11" }
			, { KeyCode.Joystick1Button12, "Joystick 1 Button 12" }
			, { KeyCode.Joystick1Button13, "Joystick 1 Button 13" }
			, { KeyCode.Joystick1Button14, "Joystick 1 Button 14" }
			, { KeyCode.Joystick1Button15, "Joystick 1 Button 15" }
			, { KeyCode.Joystick1Button16, "Joystick 1 Button 16" }
			, { KeyCode.Joystick1Button17, "Joystick 1 Button 17" }
			, { KeyCode.Joystick1Button18, "Joystick 1 Button 18" }
			, { KeyCode.Joystick1Button19, "Joystick 1 Button 19" }
			, { KeyCode.Joystick2Button0,  "Joystick 2 Button 0"  }
			, { KeyCode.Joystick2Button1,  "Joystick 2 Button 1"  }
			, { KeyCode.Joystick2Button2,  "Joystick 2 Button 2"  }
			, { KeyCode.Joystick2Button3,  "Joystick 2 Button 3"  }
			, { KeyCode.Joystick2Button4,  "Joystick 2 Button 4"  }
			, { KeyCode.Joystick2Button5,  "Joystick 2 Button 5"  }
			, { KeyCode.Joystick2Button6,  "Joystick 2 Button 6"  }
			, { KeyCode.Joystick2Button7,  "Joystick 2 Button 7"  }
			, { KeyCode.Joystick2Button8,  "Joystick 2 Button 8"  }
			, { KeyCode.Joystick2Button9,  "Joystick 2 Button 9"  }
			, { KeyCode.Joystick2Button10, "Joystick 2 Button 10" }
			, { KeyCode.Joystick2Button11, "Joystick 2 Button 11" }
			, { KeyCode.Joystick2Button12, "Joystick 2 Button 12" }
			, { KeyCode.Joystick2Button13, "Joystick 2 Button 13" }
			, { KeyCode.Joystick2Button14, "Joystick 2 Button 14" }
			, { KeyCode.Joystick2Button15, "Joystick 2 Button 15" }
			, { KeyCode.Joystick2Button16, "Joystick 2 Button 16" }
			, { KeyCode.Joystick2Button17, "Joystick 2 Button 17" }
			, { KeyCode.Joystick2Button18, "Joystick 2 Button 18" }
			, { KeyCode.Joystick2Button19, "Joystick 2 Button 19" }
			, { KeyCode.Joystick3Button0,  "Joystick 3 Button 0"  }
			, { KeyCode.Joystick3Button1,  "Joystick 3 Button 1"  }
			, { KeyCode.Joystick3Button2,  "Joystick 3 Button 2"  }
			, { KeyCode.Joystick3Button3,  "Joystick 3 Button 3"  }
			, { KeyCode.Joystick3Button4,  "Joystick 3 Button 4"  }
			, { KeyCode.Joystick3Button5,  "Joystick 3 Button 5"  }
			, { KeyCode.Joystick3Button6,  "Joystick 3 Button 6"  }
			, { KeyCode.Joystick3Button7,  "Joystick 3 Button 7"  }
			, { KeyCode.Joystick3Button8,  "Joystick 3 Button 8"  }
			, { KeyCode.Joystick3Button9,  "Joystick 3 Button 9"  }
			, { KeyCode.Joystick3Button10, "Joystick 3 Button 10" }
			, { KeyCode.Joystick3Button11, "Joystick 3 Button 11" }
			, { KeyCode.Joystick3Button12, "Joystick 3 Button 12" }
			, { KeyCode.Joystick3Button13, "Joystick 3 Button 13" }
			, { KeyCode.Joystick3Button14, "Joystick 3 Button 14" }
			, { KeyCode.Joystick3Button15, "Joystick 3 Button 15" }
			, { KeyCode.Joystick3Button16, "Joystick 3 Button 16" }
			, { KeyCode.Joystick3Button17, "Joystick 3 Button 17" }
			, { KeyCode.Joystick3Button18, "Joystick 3 Button 18" }
			, { KeyCode.Joystick3Button19, "Joystick 3 Button 19" }
			, { KeyCode.Joystick4Button0,  "Joystick 4 Button 0"  }
			, { KeyCode.Joystick4Button1,  "Joystick 4 Button 1"  }
			, { KeyCode.Joystick4Button2,  "Joystick 4 Button 2"  }
			, { KeyCode.Joystick4Button3,  "Joystick 4 Button 3"  }
			, { KeyCode.Joystick4Button4,  "Joystick 4 Button 4"  }
			, { KeyCode.Joystick4Button5,  "Joystick 4 Button 5"  }
			, { KeyCode.Joystick4Button6,  "Joystick 4 Button 6"  }
			, { KeyCode.Joystick4Button7,  "Joystick 4 Button 7"  }
			, { KeyCode.Joystick4Button8,  "Joystick 4 Button 8"  }
			, { KeyCode.Joystick4Button9,  "Joystick 4 Button 9"  }
			, { KeyCode.Joystick4Button10, "Joystick 4 Button 10" }
			, { KeyCode.Joystick4Button11, "Joystick 4 Button 11" }
			, { KeyCode.Joystick4Button12, "Joystick 4 Button 12" }
			, { KeyCode.Joystick4Button13, "Joystick 4 Button 13" }
			, { KeyCode.Joystick4Button14, "Joystick 4 Button 14" }
			, { KeyCode.Joystick4Button15, "Joystick 4 Button 15" }
			, { KeyCode.Joystick4Button16, "Joystick 4 Button 16" }
			, { KeyCode.Joystick4Button17, "Joystick 4 Button 17" }
			, { KeyCode.Joystick4Button18, "Joystick 4 Button 18" }
			, { KeyCode.Joystick4Button19, "Joystick 4 Button 19" }
			, { KeyCode.Joystick5Button0,  "Joystick 5 Button 0"  }
			, { KeyCode.Joystick5Button1,  "Joystick 5 Button 1"  }
			, { KeyCode.Joystick5Button2,  "Joystick 5 Button 2"  }
			, { KeyCode.Joystick5Button3,  "Joystick 5 Button 3"  }
			, { KeyCode.Joystick5Button4,  "Joystick 5 Button 4"  }
			, { KeyCode.Joystick5Button5,  "Joystick 5 Button 5"  }
			, { KeyCode.Joystick5Button6,  "Joystick 5 Button 6"  }
			, { KeyCode.Joystick5Button7,  "Joystick 5 Button 7"  }
			, { KeyCode.Joystick5Button8,  "Joystick 5 Button 8"  }
			, { KeyCode.Joystick5Button9,  "Joystick 5 Button 9"  }
			, { KeyCode.Joystick5Button10, "Joystick 5 Button 10" }
			, { KeyCode.Joystick5Button11, "Joystick 5 Button 11" }
			, { KeyCode.Joystick5Button12, "Joystick 5 Button 12" }
			, { KeyCode.Joystick5Button13, "Joystick 5 Button 13" }
			, { KeyCode.Joystick5Button14, "Joystick 5 Button 14" }
			, { KeyCode.Joystick5Button15, "Joystick 5 Button 15" }
			, { KeyCode.Joystick5Button16, "Joystick 5 Button 16" }
			, { KeyCode.Joystick5Button17, "Joystick 5 Button 17" }
			, { KeyCode.Joystick5Button18, "Joystick 5 Button 18" }
			, { KeyCode.Joystick5Button19, "Joystick 5 Button 19" }
			, { KeyCode.Joystick6Button0,  "Joystick 6 Button 0"  }
			, { KeyCode.Joystick6Button1,  "Joystick 6 Button 1"  }
			, { KeyCode.Joystick6Button2,  "Joystick 6 Button 2"  }
			, { KeyCode.Joystick6Button3,  "Joystick 6 Button 3"  }
			, { KeyCode.Joystick6Button4,  "Joystick 6 Button 4"  }
			, { KeyCode.Joystick6Button5,  "Joystick 6 Button 5"  }
			, { KeyCode.Joystick6Button6,  "Joystick 6 Button 6"  }
			, { KeyCode.Joystick6Button7,  "Joystick 6 Button 7"  }
			, { KeyCode.Joystick6Button8,  "Joystick 6 Button 8"  }
			, { KeyCode.Joystick6Button9,  "Joystick 6 Button 9"  }
			, { KeyCode.Joystick6Button10, "Joystick 6 Button 10" }
			, { KeyCode.Joystick6Button11, "Joystick 6 Button 11" }
			, { KeyCode.Joystick6Button12, "Joystick 6 Button 12" }
			, { KeyCode.Joystick6Button13, "Joystick 6 Button 13" }
			, { KeyCode.Joystick6Button14, "Joystick 6 Button 14" }
			, { KeyCode.Joystick6Button15, "Joystick 6 Button 15" }
			, { KeyCode.Joystick6Button16, "Joystick 6 Button 16" }
			, { KeyCode.Joystick6Button17, "Joystick 6 Button 17" }
			, { KeyCode.Joystick6Button18, "Joystick 6 Button 18" }
			, { KeyCode.Joystick6Button19, "Joystick 6 Button 19" }
			, { KeyCode.Joystick7Button0,  "Joystick 7 Button 0"  }
			, { KeyCode.Joystick7Button1,  "Joystick 7 Button 1"  }
			, { KeyCode.Joystick7Button2,  "Joystick 7 Button 2"  }
			, { KeyCode.Joystick7Button3,  "Joystick 7 Button 3"  }
			, { KeyCode.Joystick7Button4,  "Joystick 7 Button 4"  }
			, { KeyCode.Joystick7Button5,  "Joystick 7 Button 5"  }
			, { KeyCode.Joystick7Button6,  "Joystick 7 Button 6"  }
			, { KeyCode.Joystick7Button7,  "Joystick 7 Button 7"  }
			, { KeyCode.Joystick7Button8,  "Joystick 7 Button 8"  }
			, { KeyCode.Joystick7Button9,  "Joystick 7 Button 9"  }
			, { KeyCode.Joystick7Button10, "Joystick 7 Button 10" }
			, { KeyCode.Joystick7Button11, "Joystick 7 Button 11" }
			, { KeyCode.Joystick7Button12, "Joystick 7 Button 12" }
			, { KeyCode.Joystick7Button13, "Joystick 7 Button 13" }
			, { KeyCode.Joystick7Button14, "Joystick 7 Button 14" }
			, { KeyCode.Joystick7Button15, "Joystick 7 Button 15" }
			, { KeyCode.Joystick7Button16, "Joystick 7 Button 16" }
			, { KeyCode.Joystick7Button17, "Joystick 7 Button 17" }
			, { KeyCode.Joystick7Button18, "Joystick 7 Button 18" }
			, { KeyCode.Joystick7Button19, "Joystick 7 Button 19" }
			, { KeyCode.Joystick8Button0,  "Joystick 8 Button 0"  }
			, { KeyCode.Joystick8Button1,  "Joystick 8 Button 1"  }
			, { KeyCode.Joystick8Button2,  "Joystick 8 Button 2"  }
			, { KeyCode.Joystick8Button3,  "Joystick 8 Button 3"  }
			, { KeyCode.Joystick8Button4,  "Joystick 8 Button 4"  }
			, { KeyCode.Joystick8Button5,  "Joystick 8 Button 5"  }
			, { KeyCode.Joystick8Button6,  "Joystick 8 Button 6"  }
			, { KeyCode.Joystick8Button7,  "Joystick 8 Button 7"  }
			, { KeyCode.Joystick8Button8,  "Joystick 8 Button 8"  }
			, { KeyCode.Joystick8Button9,  "Joystick 8 Button 9"  }
			, { KeyCode.Joystick8Button10, "Joystick 8 Button 10" }
			, { KeyCode.Joystick8Button11, "Joystick 8 Button 11" }
			, { KeyCode.Joystick8Button12, "Joystick 8 Button 12" }
			, { KeyCode.Joystick8Button13, "Joystick 8 Button 13" }
			, { KeyCode.Joystick8Button14, "Joystick 8 Button 14" }
			, { KeyCode.Joystick8Button15, "Joystick 8 Button 15" }
			, { KeyCode.Joystick8Button16, "Joystick 8 Button 16" }
			, { KeyCode.Joystick8Button17, "Joystick 8 Button 17" }
			, { KeyCode.Joystick8Button18, "Joystick 8 Button 18" }
			, { KeyCode.Joystick8Button19, "Joystick 8 Button 19" }
		};

		/// <summary>
		/// Map of KeyCode conversions for string.
		/// </summary>
		private static readonly Dictionary<string, KeyCode> fromStringMap = new Dictionary<string, KeyCode>()
		{
			  { "None",                 KeyCode.None              }                       
			, { "Backspace",            KeyCode.Backspace         }
			, { "Del",                  KeyCode.Delete            }
			, { "Tab",                  KeyCode.Tab               }
			, { "Clear",                KeyCode.Clear             }
			, { "Return",               KeyCode.Return            }
			, { "Pause",                KeyCode.Pause             }
			, { "Esc",                  KeyCode.Escape            }
			, { "Space",                KeyCode.Space             }
			, { "Num 0",                KeyCode.Keypad0           }
			, { "Num 1",                KeyCode.Keypad1           }
			, { "Num 2",                KeyCode.Keypad2           }
			, { "Num 3",                KeyCode.Keypad3           }
			, { "Num 4",                KeyCode.Keypad4           }
			, { "Num 5",                KeyCode.Keypad5           }
			, { "Num 6",                KeyCode.Keypad6           }
			, { "Num 7",                KeyCode.Keypad7           }
			, { "Num 8",                KeyCode.Keypad8           }
			, { "Num 9",                KeyCode.Keypad9           }
			, { "Num .",                KeyCode.KeypadPeriod      }
			, { "Num /",                KeyCode.KeypadDivide      }
			, { "Num *",                KeyCode.KeypadMultiply    }
			, { "Num -",                KeyCode.KeypadMinus       }
			, { "Num +",                KeyCode.KeypadPlus        }
			, { "Enter",                KeyCode.KeypadEnter       }
			, { "Num =",                KeyCode.KeypadEquals      }
			, { "Up",                   KeyCode.UpArrow           }
			, { "Down",                 KeyCode.DownArrow         }
			, { "Right",                KeyCode.RightArrow        }
			, { "Left",                 KeyCode.LeftArrow         }
			, { "Insert",               KeyCode.Insert            }
			, { "Home",                 KeyCode.Home              }
			, { "End",                  KeyCode.End               }
			, { "Page Up",              KeyCode.PageUp            }
			, { "Page Down",            KeyCode.PageDown          }
			, { "F1",                   KeyCode.F1                }
			, { "F2",                   KeyCode.F2                }
			, { "F3",                   KeyCode.F3                }
			, { "F4",                   KeyCode.F4                }
			, { "F5",                   KeyCode.F5                }
			, { "F6",                   KeyCode.F6                }
			, { "F7",                   KeyCode.F7                }
			, { "F8",                   KeyCode.F8                }
			, { "F9",                   KeyCode.F9                }
			, { "F10",                  KeyCode.F10               }
			, { "F11",                  KeyCode.F11               }
			, { "F12",                  KeyCode.F12               }
			, { "F13",                  KeyCode.F13               }
			, { "F14",                  KeyCode.F14               }
			, { "F15",                  KeyCode.F15               }
			, { "0",                    KeyCode.Alpha0            }
			, { "1",                    KeyCode.Alpha1            }
			, { "2",                    KeyCode.Alpha2            }
			, { "3",                    KeyCode.Alpha3            }
			, { "4",                    KeyCode.Alpha4            }
			, { "5",                    KeyCode.Alpha5            }
			, { "6",                    KeyCode.Alpha6            }
			, { "7",                    KeyCode.Alpha7            }
			, { "8",                    KeyCode.Alpha8            }
			, { "9",                    KeyCode.Alpha9            }
			, { "!",                    KeyCode.Exclaim           }
			, { "\"",                   KeyCode.DoubleQuote       }
			, { "#",                    KeyCode.Hash              }
			, { "$",                    KeyCode.Dollar            }
			, { "&",                    KeyCode.Ampersand         }
			, { "'",                    KeyCode.Quote             }
			, { "(",                    KeyCode.LeftParen         }
			, { ")",                    KeyCode.RightParen        }
			, { "*",                    KeyCode.Asterisk          }
			, { "+",                    KeyCode.Plus              }
			, { ",",                    KeyCode.Comma             }
			, { "-",                    KeyCode.Minus             }
			, { ".",                    KeyCode.Period            }
			, { "/",                    KeyCode.Slash             }
			, { ":",                    KeyCode.Colon             }
			, { ";",                    KeyCode.Semicolon         }
			, { "<",                    KeyCode.Less              }
			, { "=",                    KeyCode.Equals            }
			, { ">",                    KeyCode.Greater           }
			, { "?",                    KeyCode.Question          }
			, { "@",                    KeyCode.At                }
			, { "[",                    KeyCode.LeftBracket       }
			, { "\\",                   KeyCode.Backslash         }
			, { "]",                    KeyCode.RightBracket      }
			, { "^",                    KeyCode.Caret             }
			, { "_",                    KeyCode.Underscore        }
			, { "`",                    KeyCode.BackQuote         }
			, { "A",                    KeyCode.A                 }
			, { "B",                    KeyCode.B                 }
			, { "C",                    KeyCode.C                 }
			, { "D",                    KeyCode.D                 }
			, { "E",                    KeyCode.E                 }
			, { "F",                    KeyCode.F                 }
			, { "G",                    KeyCode.G                 }
			, { "H",                    KeyCode.H                 }
			, { "I",                    KeyCode.I                 }
			, { "J",                    KeyCode.J                 }
			, { "K",                    KeyCode.K                 }
			, { "L",                    KeyCode.L                 }
			, { "M",                    KeyCode.M                 }
			, { "N",                    KeyCode.N                 }
			, { "O",                    KeyCode.O                 }
			, { "P",                    KeyCode.P                 }
			, { "Q",                    KeyCode.Q                 }
			, { "R",                    KeyCode.R                 }
			, { "S",                    KeyCode.S                 }
			, { "T",                    KeyCode.T                 }
			, { "U",                    KeyCode.U                 }
			, { "V",                    KeyCode.V                 }
			, { "W",                    KeyCode.W                 }
			, { "X",                    KeyCode.X                 }
			, { "Y",                    KeyCode.Y                 }
			, { "Z",                    KeyCode.Z                 }
			, { "Num Lock",             KeyCode.Numlock           }
			, { "Caps Lock",            KeyCode.CapsLock          }
			, { "Scroll Lock",          KeyCode.ScrollLock        }
			, { "Right Shift",          KeyCode.RightShift        }
			, { "Left Shift",           KeyCode.LeftShift         }
			, { "Right Ctrl",           KeyCode.RightControl      }
			, { "Left Ctrl",            KeyCode.LeftControl       }
			, { "Right Alt",            KeyCode.RightAlt          }
			, { "Left Alt",             KeyCode.LeftAlt           }
			, { "Left Command",         KeyCode.LeftCommand       }
			, { "Left Apple",           KeyCode.LeftApple         }
			, { "Left Windows",         KeyCode.LeftWindows       }
			, { "Right Command",        KeyCode.RightCommand      }
			, { "Right Apple",          KeyCode.RightApple        }
			, { "Right Windows",        KeyCode.RightWindows      }
			, { "AltGr",                KeyCode.AltGr             }
			, { "Help",                 KeyCode.Help              }
			, { "Print",                KeyCode.Print             }
			, { "SysReq",               KeyCode.SysReq            }
			, { "Break",                KeyCode.Break             }
			, { "Menu",                 KeyCode.Menu              }
			, { "Mouse 0",              KeyCode.Mouse0            }
			, { "Mouse 1",              KeyCode.Mouse1            }
			, { "Mouse 2",              KeyCode.Mouse2            }
			, { "Mouse 3",              KeyCode.Mouse3            }
			, { "Mouse 4",              KeyCode.Mouse4            }
			, { "Mouse 5",              KeyCode.Mouse5            }
			, { "Mouse 6",              KeyCode.Mouse6            }
			, { "Joystick Button 0",    KeyCode.JoystickButton0   }
			, { "Joystick Button 1",    KeyCode.JoystickButton1   }
			, { "Joystick Button 2",    KeyCode.JoystickButton2   }
			, { "Joystick Button 3",    KeyCode.JoystickButton3   }
			, { "Joystick Button 4",    KeyCode.JoystickButton4   }
			, { "Joystick Button 5",    KeyCode.JoystickButton5   }
			, { "Joystick Button 6",    KeyCode.JoystickButton6   }
			, { "Joystick Button 7",    KeyCode.JoystickButton7   }
			, { "Joystick Button 8",    KeyCode.JoystickButton8   }
			, { "Joystick Button 9",    KeyCode.JoystickButton9   }
			, { "Joystick Button 10",   KeyCode.JoystickButton10  }
			, { "Joystick Button 11",   KeyCode.JoystickButton11  }
			, { "Joystick Button 12",   KeyCode.JoystickButton12  }
			, { "Joystick Button 13",   KeyCode.JoystickButton13  }
			, { "Joystick Button 14",   KeyCode.JoystickButton14  }
			, { "Joystick Button 15",   KeyCode.JoystickButton15  }
			, { "Joystick Button 16",   KeyCode.JoystickButton16  }
			, { "Joystick Button 17",   KeyCode.JoystickButton17  }
			, { "Joystick Button 18",   KeyCode.JoystickButton18  }
			, { "Joystick Button 19",   KeyCode.JoystickButton19  }
			, { "Joystick 1 Button 0",  KeyCode.Joystick1Button0  }
			, { "Joystick 1 Button 1",  KeyCode.Joystick1Button1  }
			, { "Joystick 1 Button 2",  KeyCode.Joystick1Button2  }
			, { "Joystick 1 Button 3",  KeyCode.Joystick1Button3  }
			, { "Joystick 1 Button 4",  KeyCode.Joystick1Button4  }
			, { "Joystick 1 Button 5",  KeyCode.Joystick1Button5  }
			, { "Joystick 1 Button 6",  KeyCode.Joystick1Button6  }
			, { "Joystick 1 Button 7",  KeyCode.Joystick1Button7  }
			, { "Joystick 1 Button 8",  KeyCode.Joystick1Button8  }
			, { "Joystick 1 Button 9",  KeyCode.Joystick1Button9  }
			, { "Joystick 1 Button 10", KeyCode.Joystick1Button10 }
			, { "Joystick 1 Button 11", KeyCode.Joystick1Button11 }
			, { "Joystick 1 Button 12", KeyCode.Joystick1Button12 }
			, { "Joystick 1 Button 13", KeyCode.Joystick1Button13 }
			, { "Joystick 1 Button 14", KeyCode.Joystick1Button14 }
			, { "Joystick 1 Button 15", KeyCode.Joystick1Button15 }
			, { "Joystick 1 Button 16", KeyCode.Joystick1Button16 }
			, { "Joystick 1 Button 17", KeyCode.Joystick1Button17 }
			, { "Joystick 1 Button 18", KeyCode.Joystick1Button18 }
			, { "Joystick 1 Button 19", KeyCode.Joystick1Button19 }
			, { "Joystick 2 Button 0",  KeyCode.Joystick2Button0  }
			, { "Joystick 2 Button 1",  KeyCode.Joystick2Button1  }
			, { "Joystick 2 Button 2",  KeyCode.Joystick2Button2  }
			, { "Joystick 2 Button 3",  KeyCode.Joystick2Button3  }
			, { "Joystick 2 Button 4",  KeyCode.Joystick2Button4  }
			, { "Joystick 2 Button 5",  KeyCode.Joystick2Button5  }
			, { "Joystick 2 Button 6",  KeyCode.Joystick2Button6  }
			, { "Joystick 2 Button 7",  KeyCode.Joystick2Button7  }
			, { "Joystick 2 Button 8",  KeyCode.Joystick2Button8  }
			, { "Joystick 2 Button 9",  KeyCode.Joystick2Button9  }
			, { "Joystick 2 Button 10", KeyCode.Joystick2Button10 }
			, { "Joystick 2 Button 11", KeyCode.Joystick2Button11 }
			, { "Joystick 2 Button 12", KeyCode.Joystick2Button12 }
			, { "Joystick 2 Button 13", KeyCode.Joystick2Button13 }
			, { "Joystick 2 Button 14", KeyCode.Joystick2Button14 }
			, { "Joystick 2 Button 15", KeyCode.Joystick2Button15 }
			, { "Joystick 2 Button 16", KeyCode.Joystick2Button16 }
			, { "Joystick 2 Button 17", KeyCode.Joystick2Button17 }
			, { "Joystick 2 Button 18", KeyCode.Joystick2Button18 }
			, { "Joystick 2 Button 19", KeyCode.Joystick2Button19 }
			, { "Joystick 3 Button 0",  KeyCode.Joystick3Button0  }
			, { "Joystick 3 Button 1",  KeyCode.Joystick3Button1  }
			, { "Joystick 3 Button 2",  KeyCode.Joystick3Button2  }
			, { "Joystick 3 Button 3",  KeyCode.Joystick3Button3  }
			, { "Joystick 3 Button 4",  KeyCode.Joystick3Button4  }
			, { "Joystick 3 Button 5",  KeyCode.Joystick3Button5  }
			, { "Joystick 3 Button 6",  KeyCode.Joystick3Button6  }
			, { "Joystick 3 Button 7",  KeyCode.Joystick3Button7  }
			, { "Joystick 3 Button 8",  KeyCode.Joystick3Button8  }
			, { "Joystick 3 Button 9",  KeyCode.Joystick3Button9  }
			, { "Joystick 3 Button 10", KeyCode.Joystick3Button10 }
			, { "Joystick 3 Button 11", KeyCode.Joystick3Button11 }
			, { "Joystick 3 Button 12", KeyCode.Joystick3Button12 }
			, { "Joystick 3 Button 13", KeyCode.Joystick3Button13 }
			, { "Joystick 3 Button 14", KeyCode.Joystick3Button14 }
			, { "Joystick 3 Button 15", KeyCode.Joystick3Button15 }
			, { "Joystick 3 Button 16", KeyCode.Joystick3Button16 }
			, { "Joystick 3 Button 17", KeyCode.Joystick3Button17 }
			, { "Joystick 3 Button 18", KeyCode.Joystick3Button18 }
			, { "Joystick 3 Button 19", KeyCode.Joystick3Button19 }
			, { "Joystick 4 Button 0",  KeyCode.Joystick4Button0  }
			, { "Joystick 4 Button 1",  KeyCode.Joystick4Button1  }
			, { "Joystick 4 Button 2",  KeyCode.Joystick4Button2  }
			, { "Joystick 4 Button 3",  KeyCode.Joystick4Button3  }
			, { "Joystick 4 Button 4",  KeyCode.Joystick4Button4  }
			, { "Joystick 4 Button 5",  KeyCode.Joystick4Button5  }
			, { "Joystick 4 Button 6",  KeyCode.Joystick4Button6  }
			, { "Joystick 4 Button 7",  KeyCode.Joystick4Button7  }
			, { "Joystick 4 Button 8",  KeyCode.Joystick4Button8  }
			, { "Joystick 4 Button 9",  KeyCode.Joystick4Button9  }
			, { "Joystick 4 Button 10", KeyCode.Joystick4Button10 }
			, { "Joystick 4 Button 11", KeyCode.Joystick4Button11 }
			, { "Joystick 4 Button 12", KeyCode.Joystick4Button12 }
			, { "Joystick 4 Button 13", KeyCode.Joystick4Button13 }
			, { "Joystick 4 Button 14", KeyCode.Joystick4Button14 }
			, { "Joystick 4 Button 15", KeyCode.Joystick4Button15 }
			, { "Joystick 4 Button 16", KeyCode.Joystick4Button16 }
			, { "Joystick 4 Button 17", KeyCode.Joystick4Button17 }
			, { "Joystick 4 Button 18", KeyCode.Joystick4Button18 }
			, { "Joystick 4 Button 19", KeyCode.Joystick4Button19 }
			, { "Joystick 5 Button 0",  KeyCode.Joystick5Button0  }
			, { "Joystick 5 Button 1",  KeyCode.Joystick5Button1  }
			, { "Joystick 5 Button 2",  KeyCode.Joystick5Button2  }
			, { "Joystick 5 Button 3",  KeyCode.Joystick5Button3  }
			, { "Joystick 5 Button 4",  KeyCode.Joystick5Button4  }
			, { "Joystick 5 Button 5",  KeyCode.Joystick5Button5  }
			, { "Joystick 5 Button 6",  KeyCode.Joystick5Button6  }
			, { "Joystick 5 Button 7",  KeyCode.Joystick5Button7  }
			, { "Joystick 5 Button 8",  KeyCode.Joystick5Button8  }
			, { "Joystick 5 Button 9",  KeyCode.Joystick5Button9  }
			, { "Joystick 5 Button 10", KeyCode.Joystick5Button10 }
			, { "Joystick 5 Button 11", KeyCode.Joystick5Button11 }
			, { "Joystick 5 Button 12", KeyCode.Joystick5Button12 }
			, { "Joystick 5 Button 13", KeyCode.Joystick5Button13 }
			, { "Joystick 5 Button 14", KeyCode.Joystick5Button14 }
			, { "Joystick 5 Button 15", KeyCode.Joystick5Button15 }
			, { "Joystick 5 Button 16", KeyCode.Joystick5Button16 }
			, { "Joystick 5 Button 17", KeyCode.Joystick5Button17 }
			, { "Joystick 5 Button 18", KeyCode.Joystick5Button18 }
			, { "Joystick 5 Button 19", KeyCode.Joystick5Button19 }
			, { "Joystick 6 Button 0",  KeyCode.Joystick6Button0  }
			, { "Joystick 6 Button 1",  KeyCode.Joystick6Button1  }
			, { "Joystick 6 Button 2",  KeyCode.Joystick6Button2  }
			, { "Joystick 6 Button 3",  KeyCode.Joystick6Button3  }
			, { "Joystick 6 Button 4",  KeyCode.Joystick6Button4  }
			, { "Joystick 6 Button 5",  KeyCode.Joystick6Button5  }
			, { "Joystick 6 Button 6",  KeyCode.Joystick6Button6  }
			, { "Joystick 6 Button 7",  KeyCode.Joystick6Button7  }
			, { "Joystick 6 Button 8",  KeyCode.Joystick6Button8  }
			, { "Joystick 6 Button 9",  KeyCode.Joystick6Button9  }
			, { "Joystick 6 Button 10", KeyCode.Joystick6Button10 }
			, { "Joystick 6 Button 11", KeyCode.Joystick6Button11 }
			, { "Joystick 6 Button 12", KeyCode.Joystick6Button12 }
			, { "Joystick 6 Button 13", KeyCode.Joystick6Button13 }
			, { "Joystick 6 Button 14", KeyCode.Joystick6Button14 }
			, { "Joystick 6 Button 15", KeyCode.Joystick6Button15 }
			, { "Joystick 6 Button 16", KeyCode.Joystick6Button16 }
			, { "Joystick 6 Button 17", KeyCode.Joystick6Button17 }
			, { "Joystick 6 Button 18", KeyCode.Joystick6Button18 }
			, { "Joystick 6 Button 19", KeyCode.Joystick6Button19 }
			, { "Joystick 7 Button 0",  KeyCode.Joystick7Button0  }
			, { "Joystick 7 Button 1",  KeyCode.Joystick7Button1  }
			, { "Joystick 7 Button 2",  KeyCode.Joystick7Button2  }
			, { "Joystick 7 Button 3",  KeyCode.Joystick7Button3  }
			, { "Joystick 7 Button 4",  KeyCode.Joystick7Button4  }
			, { "Joystick 7 Button 5",  KeyCode.Joystick7Button5  }
			, { "Joystick 7 Button 6",  KeyCode.Joystick7Button6  }
			, { "Joystick 7 Button 7",  KeyCode.Joystick7Button7  }
			, { "Joystick 7 Button 8",  KeyCode.Joystick7Button8  }
			, { "Joystick 7 Button 9",  KeyCode.Joystick7Button9  }
			, { "Joystick 7 Button 10", KeyCode.Joystick7Button10 }
			, { "Joystick 7 Button 11", KeyCode.Joystick7Button11 }
			, { "Joystick 7 Button 12", KeyCode.Joystick7Button12 }
			, { "Joystick 7 Button 13", KeyCode.Joystick7Button13 }
			, { "Joystick 7 Button 14", KeyCode.Joystick7Button14 }
			, { "Joystick 7 Button 15", KeyCode.Joystick7Button15 }
			, { "Joystick 7 Button 16", KeyCode.Joystick7Button16 }
			, { "Joystick 7 Button 17", KeyCode.Joystick7Button17 }
			, { "Joystick 7 Button 18", KeyCode.Joystick7Button18 }
			, { "Joystick 7 Button 19", KeyCode.Joystick7Button19 }
			, { "Joystick 8 Button 0",  KeyCode.Joystick8Button0  }
			, { "Joystick 8 Button 1",  KeyCode.Joystick8Button1  }
			, { "Joystick 8 Button 2",  KeyCode.Joystick8Button2  }
			, { "Joystick 8 Button 3",  KeyCode.Joystick8Button3  }
			, { "Joystick 8 Button 4",  KeyCode.Joystick8Button4  }
			, { "Joystick 8 Button 5",  KeyCode.Joystick8Button5  }
			, { "Joystick 8 Button 6",  KeyCode.Joystick8Button6  }
			, { "Joystick 8 Button 7",  KeyCode.Joystick8Button7  }
			, { "Joystick 8 Button 8",  KeyCode.Joystick8Button8  }
			, { "Joystick 8 Button 9",  KeyCode.Joystick8Button9  }
			, { "Joystick 8 Button 10", KeyCode.Joystick8Button10 }
			, { "Joystick 8 Button 11", KeyCode.Joystick8Button11 }
			, { "Joystick 8 Button 12", KeyCode.Joystick8Button12 }
			, { "Joystick 8 Button 13", KeyCode.Joystick8Button13 }
			, { "Joystick 8 Button 14", KeyCode.Joystick8Button14 }
			, { "Joystick 8 Button 15", KeyCode.Joystick8Button15 }
			, { "Joystick 8 Button 16", KeyCode.Joystick8Button16 }
			, { "Joystick 8 Button 17", KeyCode.Joystick8Button17 }
			, { "Joystick 8 Button 18", KeyCode.Joystick8Button18 }
			, { "Joystick 8 Button 19", KeyCode.Joystick8Button19 }
		};

		/// <summary>
		/// Initializes the <see cref="Internal.KeyCodeConversions"/> class.
		/// </summary>
		static KeyCodeConversions()
		{
			string[] keyCodes = Enum.GetNames(typeof(KeyCode));

			if (
				keyCodes.Length != toStringMap.Count + 2 // Two duplicates for Apple keys
				||
				keyCodes.Length != fromStringMap.Count
			   )
			{
				Debug.LogError("KeyCode to string conversion may fail, please contact with developer: Gris87@yandex.ru");
			}
		}

		/// <summary>
		/// Converts specified KeyCode to string.
		/// </summary>
		/// <returns>The string representation.</returns>
		/// <param name="keyCode">Key code.</param>
		public static string toString(KeyCode keyCode)
		{
			string res;
			
			if (toStringMap.TryGetValue(keyCode, out res))
			{
				return res;
			}

			Debug.LogError("Failed to convert KeyCode \"" + keyCode.ToString() + "\" to string");

			return keyCode.ToString();
		}

		/// <summary>
		/// Converts specified string to KeyCode.
		/// </summary>
		/// <returns>Key code.</param>
		/// <param name="value">The string representation.</returns>
		public static KeyCode fromString(string value)
		{
			KeyCode res;
			
			if (fromStringMap.TryGetValue(value, out res))
			{
				return res;
			}

			try
			{
				KeyCode key = (KeyCode)Enum.Parse(typeof(KeyCode), value);
				return key;
			}
			catch (Exception)
			{
			}

			Debug.LogError("Failed to convert string \"" + value + "\" to KeyCode");
			throw new Exception();
		}
	}
}
#endregion



/// <summary>
/// <see cref="KeyboardInput"/> handles keyboard input device.
/// Source: https://github.com/Gris87/InputControl
/// </summary>
public class KeyboardInput : CustomInput
{
    private KeyCode mKey;

    private string  mCachedToString;



    #region Properties

    #region key
    /// <summary>
    /// Gets the keyboard key.
    /// </summary>
    /// <value>Keyboard key.</value>
    public KeyCode key
    {
        get
        {
            return mKey;
        }
    }
    #endregion

    #endregion



    /// <summary>
    /// Create a new instance of <see cref="KeyboardInput"/> that handles specified keyboard key.
    /// </summary>
    /// <param name="key">Keyboard key.</param>
    /// <param name="modifiers">Key modifiers.</param>
    public KeyboardInput(KeyCode key = KeyCode.None, KeyModifier modifiers = KeyModifier.NoModifier)
    {
        mKey       = key;
        mModifiers = modifiers;

        mCachedToString = null;
    }

    /// <summary>
    /// Parse string argument and try to create <see cref="KeyboardInput"/> instance.
    /// </summary>
    /// <returns>Parsed KeyboardInput.</returns>
    /// <param name="value">String representation of KeyboardInput.</param>
    public static KeyboardInput FromString(string value)
    {
		if (value == null)
		{
			return null;
		}

        KeyModifier modifiers = modifiersFromString(ref value);

        try
        {
            return new KeyboardInput(Internal.KeyCodeConversions.fromString(value), modifiers);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents the current <see cref="KeyboardInput"/>.
    /// </summary>
    /// <returns>A <see cref="System.String"/> that represents the current <see cref="KeyboardInput"/>.</returns>
    public override string ToString()
    {
        if (mCachedToString == null)
        {
            mCachedToString = modifiersToString() + Internal.KeyCodeConversions.toString(mKey);
        }

        return mCachedToString;
    }

    /// <summary>
    /// Returns input value while the user holds down the key.
    /// </summary>
    /// <returns>Input value if button or axis is still active.</returns>
    /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
    /// <param name="axis">Specific actions for axis (Empty by default).</param>
    /// <param name="device">Preferred input device.</param>
    public override float getInput(bool exactKeyModifiers = false, string axis = "", InputDevice device = InputDevice.Any)
    {
        if (
            device != InputDevice.Any
            &&
            device != InputDevice.KeyboardAndMouse
            ||
            !checkModifiersForKeys(exactKeyModifiers)
           )
        {
            return 0;
        }

        float sensitivity = 1;

        if (
            axis != null
            &&
            (
             axis.Equals("Mouse X")
             ||
             axis.Equals("Mouse Y")
            )
           )
        {
            sensitivity = 0.1f;
        }

        return Input.GetKey(mKey) ? sensitivity : 0;
    }

    /// <summary>
    /// Returns input value during the frame the user starts pressing down the key.
    /// </summary>
    /// <returns>Input value if button or axis become active during this frame.</returns>
    /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
    /// <param name="axis">Specific actions for axis (Empty by default).</param>
    /// <param name="device">Preferred input device.</param>
    public override float getInputDown(bool exactKeyModifiers = false, string axis = "", InputDevice device = InputDevice.Any)
    {
        if (
            device != InputDevice.Any
            &&
            device != InputDevice.KeyboardAndMouse
            ||
            !checkModifiersForKeys(exactKeyModifiers)
           )
        {
            return 0;
        }

        float sensitivity = 1;

        if (
            axis != null
            &&
            (
             axis.Equals("Mouse X")
             ||
             axis.Equals("Mouse Y")
            )
           )
        {
            sensitivity = 0.1f;
        }

        return Input.GetKeyDown(mKey) ? sensitivity : 0;
    }

    /// <summary>
    /// Returns input value during the frame the user releases the key.
    /// </summary>
    /// <returns>Input value if button or axis stopped being active during this frame.</returns>
    /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
    /// <param name="axis">Specific actions for axis (Empty by default).</param>
    /// <param name="device">Preferred input device.</param>
    public override float getInputUp(bool exactKeyModifiers = false, string axis = "", InputDevice device = InputDevice.Any)
    {
        if (
            device != InputDevice.Any
            &&
            device != InputDevice.KeyboardAndMouse
            ||
            !checkModifiersForKeys(exactKeyModifiers)
           )
        {
            return 0;
        }

        float sensitivity = 1;

        if (
            axis != null
            &&
            (
             axis.Equals("Mouse X")
             ||
             axis.Equals("Mouse Y")
            )
           )
        {
            sensitivity = 0.1f;
        }

        return Input.GetKeyUp(mKey) ? sensitivity : 0;
    }

    /// <summary>
    /// Verifies that specified key modifiers are active during current frame.
    /// </summary>
    /// <returns>Specified key modifiers are active during current frame.</returns>
    /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
    private bool checkModifiersForKeys(bool exactKeyModifiers = false)
    {
        if (!exactKeyModifiers && mModifiers == KeyModifier.NoModifier)
        {
            return true;
        }

        if (mCachedModifiersFrame != Time.frameCount)
        {
            KeyModifier res = KeyModifier.NoModifier;

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                res |= KeyModifier.Ctrl;
            }

            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                res |= KeyModifier.Alt;
            }

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                res |= KeyModifier.Shift;
            }

            mCachedModifiersFrame = Time.frameCount;
            mCachedModifiersState = res;
        }

        if (exactKeyModifiers)
        {
            if (
                mKey == KeyCode.LeftControl
                ||
                mKey == KeyCode.RightControl
               )
            {
                return (mModifiers | KeyModifier.Ctrl) == mCachedModifiersState;
            }

            if (
                mKey == KeyCode.LeftAlt
                ||
                mKey == KeyCode.RightAlt
               )
            {
                return (mModifiers | KeyModifier.Alt) == mCachedModifiersState;
            }

            if (
                mKey == KeyCode.LeftShift
                ||
                mKey == KeyCode.RightShift
               )
            {
                return (mModifiers | KeyModifier.Shift) == mCachedModifiersState;
            }
        }
        else
        {
            return (mModifiers & mCachedModifiersState) == mModifiers;
        }

        return mModifiers == mCachedModifiersState;
    }
}
