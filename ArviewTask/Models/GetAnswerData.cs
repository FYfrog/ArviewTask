namespace ArviewTask.Models
{
  public class GetAnswerData
  {
    public string GuessedWord { get; }
    
    public int AttemptNumber { get; }
    
    public string HiddenWord { get; }
    
    public int LastSuggestedLetterNumber { get; }

    public GetAnswerData(string guessedWord, int attemptNumber, string hiddenWord, int lastSuggestedLetterNumber)
    {
      GuessedWord = guessedWord;
      AttemptNumber = attemptNumber;
      HiddenWord = hiddenWord;
      LastSuggestedLetterNumber = lastSuggestedLetterNumber;
    }
  }
}