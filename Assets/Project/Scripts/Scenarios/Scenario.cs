using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScenarioReward : Reward
{
    public string ScenarioID;
    public ScenarioReward(string scenarioId, ItemData item, int quantity) : base(item, quantity)
    {
        ScenarioID = scenarioId;
    }
}

public class Reward
{
    public ItemData Item;
    public int Quantity;
    public Reward(ItemData item, int quantity)
    {
        Item = item;
        Quantity = quantity;
    }
}
public class ScenarioStep
{
    public List<Quiz> Quizzes;
    public int Tries;
    public QuizState State;

    public ScenarioStep(List<Quiz> quizzes, int tries)
    {
        Quizzes = quizzes;
        Tries = tries;
        State = QuizState.Pending;
    }
}

[System.Serializable]
public class Scenario
{
    public string ID { get; private set; }
    public string Name { get; private set; }
    public Isle Isle { get; private set; }
    public List<ScenarioReward> Rewards { get; private set; }
    public int Successes { get; set; }
    public int Failures { get; set; }
    public int RewardReceived { get; set; }
    public int Restarted { get; set; }


    public List<ScenarioStep> Steps;
    public Sprite PreviewSprite { get; private set; }
    public bool Locked { get; set; }
    public bool IsSpecial { get; private set; }

    public Scenario()
    {

    }

    public Scenario(string id, string name, Isle isle, List<ScenarioReward> rewards, bool isSpecial = false)
    {
        ID = id;
        Name = name;
        Isle = isle;
        Rewards = rewards;
        Successes = 0;
        Failures = 0;
        RewardReceived = 0;
        Restarted = 0;
        Locked = false;
        Steps = new List<ScenarioStep>();
        IsSpecial = isSpecial;
    }

    public Scenario(string id, string name, Isle isle, List<ScenarioReward> rewards, int successes, int failures, int rewardReceived, int restarted, bool locked, bool isSpecial = false)
    {
        ID = id;
        Name = name;
        Isle = isle;
        Rewards = rewards;
        Successes = successes;
        Failures = failures;
        RewardReceived = rewardReceived;
        Restarted = restarted;
        Locked = locked;
        Steps = new List<ScenarioStep>();
        IsSpecial = isSpecial;
    }

    /// <summary>Add a list of Quiz (Main+Variant) in their corresponding step.<br/>
    /// Works only for a list of quiz ordered by their parent id</summary>
    /// <param name="quizzes">List of quiz</param>
    public void AddQuizzes(List<Quiz> quizzes)
    {
        // Work only for list ordered by parent id /!\
        int stepDone = 0;
        while (stepDone < Steps.Count)
        {
            Steps[stepDone].Quizzes = quizzes.FindAll(quiz => quiz.ID == quizzes[stepDone].ID || quiz.ParentID == quizzes[stepDone].ID);
            stepDone++;
        }
    }

    /// <summary>Return a quiz for the given step index.<br/>
    /// The returned quiz depends on the number of try for this step</summary>
    /// <param name="index">Step's index</param>
    /// <returns>Scenario.Steps[index]'s Quiz</returns>
    public Quiz GetQuizOfStep(int index)
    {
        // Modulo to get the corresponding quiz between itself and variants
        int calculatedQuizIndex = 0;
        if (Steps[index].Quizzes.Count > 0)
        {
            // return last completed quiz
            if (Steps[index].State == QuizState.Completed && Steps[index].Tries > 0)
            {
                calculatedQuizIndex = (Steps[index].Tries - 1) % (Steps[index].Quizzes.Count);
            }
            else
            {
                // return next quiz
                calculatedQuizIndex = Steps[index].Tries % (Steps[index].Quizzes.Count);
            }
        }
        else
        {
            Debug.LogError("0 Quiz Founds");
        }
        return Steps[index].Quizzes[calculatedQuizIndex];
    }

    /// <param name="quiz">Quiz</param>
    /// <returns>Scenario.Steps's index of the given Quiz</returns>
    public int GetStepIndexOfQuiz(Quiz quiz)
    {
        for (int i = 0; i < Steps.Count; i++)
        {
            if (Steps[i].Quizzes.FindIndex(q => q.ID == quiz.ID) != -1)
            {
                return i;
            }
        }
        return -1; // TODO : If it comes here : error
    }

    /// <summary>Return quiz with the given id by searching the steps</summary>
    /// <param name="id">Quiz's ID</param>
    public Quiz FindQuizById(string id)
    {
        Quiz foundQuiz = null;
        foreach (ScenarioStep step in Steps)
        {
            foundQuiz = step.Quizzes.Find(quiz => quiz.ID == id);
            if (foundQuiz != null)
            {
                return foundQuiz;
            }
        }
        return foundQuiz;
    }

    /// <summary>Return true if all steps are completed else return false.
    /// Work only after loading quiz related to this scenario</summary>
    public bool Completed()
    {
        foreach (ScenarioStep step in Steps)
        {
            if (step.State != QuizState.Completed)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>Return true if there is no step with the pending state else return false.
    /// Work only after loading quiz related to this scenario</summary>
    public bool Tried()
    {
        foreach (ScenarioStep step in Steps)
        {
            if (step.State == QuizState.Pending)
            {
                return false;
            }
        }
        return true;
    }

    public int CurrentCompletions
    {
        get { return Successes; }
    }
}
