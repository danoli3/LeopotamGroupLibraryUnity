//-------------------------------------------------------
// LeopotamGroupLibrary for unity3d
// Copyright (c) 2012-2016 Leopotam <leopotam@gmail.com>
//-------------------------------------------------------
// Autogenerated with Coco/R, dont change it manually.
//-------------------------------------------------------

using System;
using System.Collections.Generic;

namespace LeopotamGroup.Scripting.Internal {
class Token {
	public int kind;    // token kind
	public string val;  // token value
	public int pos;     // token position in bytes in the source text (starting at 0)
	public int charPos;  // token position in characters in the source text (starting at 0)
	public int col;     // token column (starting at 1)
	public int line;    // token line (starting at 1)
}

//-----------------------------------------------------------------------------------
// Scanner
//-----------------------------------------------------------------------------------
class Scanner {
	const char EOL = '\n';
	const int eofSym = 0; /* pdt */
	const int maxT = 23;
	const int noSym = 23;


	public static readonly Token EmptyToken = new Token { kind = 0, val = "" };

	readonly List<Token> _tokens = new List<Token> (512);
    public int PC;

	class Buffer {
		public int Pos;
		public string Data;
		public const int EOF = '\0';

		public int Read() {
			if (Pos >= Data.Length) {
				return EOF;
			}
			return Data[Pos++];
		}

		public int Peek() {
			if ((Pos + 1) >= Data.Length) {
				return EOF;
			}
			return Data[Pos + 1];
		}
	}
	readonly Buffer buffer = new Buffer(); // scanner buffer
	Token t;          // current token
	int ch;           // current input character
	int pos;          // byte position of current character
	int charPos;      // position by unicode characters starting with 0
	int col;          // column number of current character
	int line;         // line number of current character
	int oldEols;      // EOLs that appeared in a comment;
	static readonly Dictionary<int, int> start; // maps first token character to start state

	char[] tval = new char[128]; // text of current token
	int tlen;         // length of current token
	
	static Scanner() {
		start = new Dictionary<int, int> (128);
		for (int i = 65; i <= 90; ++i) start[i] = 1;
		for (int i = 95; i <= 95; ++i) start[i] = 1;
		for (int i = 97; i <= 122; ++i) start[i] = 1;
		for (int i = 48; i <= 57; ++i) start[i] = 2;
		start[39] = 5; 
		start[40] = 7; 
		start[44] = 8; 
		start[41] = 9; 
		start[123] = 10; 
		start[125] = 11; 
		start[59] = 12; 
		start[60] = 13; 
		start[61] = 20; 
		start[62] = 15; 
		start[43] = 16; 
		start[45] = 17; 
		start[42] = 18; 
		start[47] = 19; 
		start[Buffer.EOF] = -1;

	}

	public string Load (string s) {
		if (string.IsNullOrEmpty (s)) {
            return "No source code";
        }
		buffer.Data = s;
		Reset();

		try {
	        Token t;
	        while (true) {
	            t = NextToken ();
	            if (t.kind == 0) {
	                break;
	            }
	            if (t.kind == Parser._STRING) {
	                t.val = t.val.Substring (1, t.val.Length - 2);
	            }
	            _tokens.Add (t);
	            //Console.WriteLine ("token " + (_tokens.Count - 1) + " : " + t.kind + " => " + t.val);
	        }
        } catch (Exception ex) {
        	return ex.Message;
        }
        return null;
	}

    public Token Scan () {
        var token = Peek();
        PC++;
        return token;
    }

    public Token Peek() {
        if (PC < 0 || PC >= _tokens.Count) {
            return Scanner.EmptyToken;
        }
        return _tokens[PC];
    }

	public void Reset() {
		PC = 0;
        _tokens.Clear ();
		buffer.Pos = 0;
		pos = -1; line = 1; col = 0; charPos = -1;
		oldEols = 0;
		NextCh();
	}

	void NextCh() {
		if (oldEols > 0) { ch = EOL; oldEols--; } 
		else {
			pos = buffer.Pos;
			// buffer reads unicode chars, if UTF8 has been detected
			ch = buffer.Read();
			col++;
			charPos++;
			// replace isolated '\r' by '\n' in order to make
			// eol handling uniform across Windows, Unix and Mac
			if (ch == '\r' && buffer.Peek() != '\n') {
				ch = EOL;
			}
			if (ch == EOL) {
				line++;
				col = 0;
			}
		}

	}

	void AddCh() {
		if (tlen >= tval.Length) {
			char[] newBuf = new char[2 * tval.Length];
			Array.Copy(tval, 0, newBuf, 0, tval.Length);
			tval = newBuf;
		}
		if (ch != Buffer.EOF) {
			tval[tlen++] = (char) ch;
			NextCh();
		}
	}



	bool Comment0() {
		int level = 1, pos0 = pos, line0 = line, col0 = col, charPos0 = charPos;
		NextCh();
		if (ch == '/') {
			NextCh();
			for(;;) {
				if (ch == 10) {
					level--;
					if (level == 0) { oldEols = line - line0; NextCh(); return true; }
					NextCh();
				} else if (ch == Buffer.EOF) return false;
				else NextCh();
			}
		} else {
			buffer.Pos = pos0; NextCh(); line = line0; col = col0; charPos = charPos0;
		}
		return false;
	}

	bool Comment1() {
		int level = 1, pos0 = pos, line0 = line, col0 = col, charPos0 = charPos;
		NextCh();
		if (ch == '*') {
			NextCh();
			for(;;) {
				if (ch == '*') {
					NextCh();
					if (ch == '/') {
						level--;
						if (level == 0) { oldEols = line - line0; NextCh(); return true; }
						NextCh();
					}
				} else if (ch == '/') {
					NextCh();
					if (ch == '*') {
						level++; NextCh();
					}
				} else if (ch == Buffer.EOF) return false;
				else NextCh();
			}
		} else {
			buffer.Pos = pos0; NextCh(); line = line0; col = col0; charPos = charPos0;
		}
		return false;
	}


	void CheckLiteral() {
		switch (t.val) {
			case "function": t.kind = 4; break;
			case "return": t.kind = 10; break;
			case "var": t.kind = 19; break;
			case "if": t.kind = 21; break;
			case "else": t.kind = 22; break;
			default: break;
		}
	}

	// get the next token (possibly a token already seen during peeking)
	public Token NextToken() {
		while (ch == ' ' ||
			ch >= 9 && ch <= 10 || ch == 13
		) NextCh();
		if (ch == '/' && Comment0() ||ch == '/' && Comment1()) return NextToken();
		int recKind = noSym;
		int recEnd = pos;
		t = new Token();
		t.pos = pos; t.col = col; t.line = line; t.charPos = charPos;
		int state;
		if (start.ContainsKey(ch)) { state = (int) start[ch]; }
		else { state = 0; }
		tlen = 0; AddCh();
		
		switch (state) {
			case -1: { t.kind = eofSym; break; } // NextCh already done
			case 0: {
				if (recKind != noSym) {
					tlen = recEnd - t.pos;
					SetScannerBehindT();
				}
				t.kind = recKind; break;
			} // NextCh already done
			case 1:
				recEnd = pos; recKind = 1;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 1;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 2:
				recEnd = pos; recKind = 2;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 2;}
				else if (ch == '.') {AddCh(); goto case 3;}
				else {t.kind = 2; break;}
			case 3:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 4;}
				else {goto case 0;}
			case 4:
				recEnd = pos; recKind = 2;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 4;}
				else {t.kind = 2; break;}
			case 5:
				if (ch <= '&' || ch >= '(' && ch <= 65535) {AddCh(); goto case 5;}
				else if (ch == 39) {AddCh(); goto case 6;}
				else {goto case 0;}
			case 6:
				{t.kind = 3; break;}
			case 7:
				{t.kind = 5; break;}
			case 8:
				{t.kind = 6; break;}
			case 9:
				{t.kind = 7; break;}
			case 10:
				{t.kind = 8; break;}
			case 11:
				{t.kind = 9; break;}
			case 12:
				{t.kind = 11; break;}
			case 13:
				{t.kind = 12; break;}
			case 14:
				{t.kind = 13; break;}
			case 15:
				{t.kind = 14; break;}
			case 16:
				{t.kind = 15; break;}
			case 17:
				{t.kind = 16; break;}
			case 18:
				{t.kind = 17; break;}
			case 19:
				{t.kind = 18; break;}
			case 20:
				recEnd = pos; recKind = 20;
				if (ch == '=') {AddCh(); goto case 14;}
				else {t.kind = 20; break;}

		}
		t.val = new String(tval, 0, tlen);
		return t;
	}
	
	private void SetScannerBehindT() {
		buffer.Pos = t.pos;
		NextCh();
		line = t.line; col = t.col; charPos = t.charPos;
		for (int i = 0; i < tlen; i++) NextCh();
	}
}}