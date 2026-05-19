using System;
using System.Collections.Generic;

[Serializable]
public class QuizQuestion
{
    public string question;
    public string answerId;
}

[Serializable]
public class QuizQuestionList
{
    public List<QuizQuestion> questions;
}