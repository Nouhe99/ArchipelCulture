using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Answer
{
    public Answer(string id, string quizId, string label, bool correct)
    {
        ID = id;
        QuizId = quizId;
        Label = label;
        Correct = correct;
    }
    public string ID;
    public string QuizId;
    public string Label;
    public bool Correct;
}
public enum QuizState
{
    Pending,
    Failed,
    Completed,
    None
}

public class Quiz
{
    public string ID { get; private set; }
    public string ParentID { get; private set; }
    public string Title { get; private set; }
    public string Level { get; private set; }
    public Notion Notion { get; private set; }
    public string Question { get; private set; }
    public Sprite QuestionImage { get; private set; }
    public string Correction { get; private set; }
    public Sprite CorrectionImage { get; private set; }
    public List<Answer> Answers { get; private set; }
    public bool HasMultipleChoice { get; private set; }
    public string VideoUrl { get; private set; }

    public QuizState State { get; set; }

    public Quiz(string id, string title, string level, Notion notion, string question, Sprite questionImage, string correction, Sprite correctionImage, List<Answer> answers, bool hasMultipleChoice, string videoUrl, string parentId)
    {
        ID = id;
        Title = title;
        Level = level;
        Notion = notion;
        Question = question;
        QuestionImage = questionImage;
        Correction = correction;
        CorrectionImage = correctionImage;
        Answers = answers;
        HasMultipleChoice = hasMultipleChoice;
        VideoUrl = videoUrl;
        ParentID = parentId;
        State = QuizState.Pending;
    }
}

