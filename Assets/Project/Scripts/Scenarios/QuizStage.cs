using System.Collections.Generic;
using UnityEngine;

public class QuizStage : ScenarioStage
{
    public string Description { get; private set; }
    public string Question { get; private set; }
    public Sprite QuestionPicture { get; private set; }
    public bool MultipleChoice { get; private set; }
    public List<Answer> Answers { get; private set; }
    public string Explanation { get; private set; }
    public Sprite ExplanationPicture { get; private set; }
    public bool HasVideo { get; private set; }
    public VideoStage Video { get; set; }
    public string LastModified { get; private set; }


    public QuizStage() : base()
    {

    }

    public QuizStage(ScenarioStageType type, string title, string description, string question, Sprite questionPicture, bool multipleChoice, List<Answer> answers, string explanation, Sprite explanationPicture, bool hasVideo, int stageId, int scenarioId, string lastModified) : base(type, title, stageId, scenarioId)
    {
        Description = description;
        Question = question;
        QuestionPicture = questionPicture;
        MultipleChoice = multipleChoice;
        Answers = answers;
        Explanation = explanation;
        ExplanationPicture = explanationPicture;
        HasVideo = hasVideo;
        LastModified = lastModified;
    }

    public QuizStage(ScenarioStageType type, string title, string description, string question, Sprite questionPicture, bool multipleChoice, List<Answer> answers, string explanation, Sprite explanationPicture, bool hasVideo, int stageId, int scenarioId, string lastModified, VideoStage video) : this(type, title, description, question, questionPicture, multipleChoice, answers, explanation, explanationPicture, hasVideo, stageId, scenarioId, lastModified)
    {
        Video = video;
    }
}
