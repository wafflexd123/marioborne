using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Console : MonoBehaviour
{
    public static bool Enabled { get => singleton != null && singleton.text.enabled; }
    static Console singleton;
    static readonly List<Line> lines = new List<Line>();
    static readonly StringBuilder stringBuilder = new StringBuilder();
    public Text text;
    public bool keepPlaceholderText;

    public static Line AddLine(string text = "")
    {
        Line line = new Line(text);
        lines.Add(line);
        return line;
    }

    private void Awake()
    {
        lines.Clear();
        stringBuilder.Clear();
        singleton = this;
        if (keepPlaceholderText) AddLine(text.text);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            text.enabled = !text.enabled;
        }
        if (text.enabled)
        {
            stringBuilder.Clear();
            for (int i = 0; i < lines.Count; i++)
            {
                stringBuilder.AppendLine(lines[i].text);
            }
            text.text = stringBuilder.ToString();
        }
    }

    public class Line
    {
        public string text;

        public Line(string text)
        {
            this.text = text;
        }
    }
}
