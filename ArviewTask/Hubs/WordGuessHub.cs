using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArviewTask.Models;
using Microsoft.AspNetCore.SignalR;

namespace ArviewTask.Hubs
{
  public class WordGuessHub : Hub
  {
    private static readonly List<string> Words = new List<string>
    {
      "ягода",
      "трансформатор",
      "хлоргексидин",
      "пиво",
      "кашалот"
    };
    
    private static readonly List<string> SimpleAnswers = new List<string>
    {
      "Нет",
      "Не угадал",
      "Попробуй ещё раз",
    };
    
    private static readonly List<string> HumiliatingAnswers = new List<string>
    {
      "Где ты такое слово взял?",
      "Мдааа",
      "Может лучше в доту?",
      "Вращайте барабан!",
    };
    
    private static readonly Random RandomGenerator = new Random();
    
    public async Task SendMessage(string rawGuessedWord)
    {
      var cleanGuessedWord = rawGuessedWord.Trim().ToLowerInvariant();
      if (cleanGuessedWord.Length == 0)
        return;
      
      await Clients.All.SendAsync("ReceiveMessage", "Вы", cleanGuessedWord);

      TryInitGame();

      await Task.Delay(500);

      var attemptNumber = (int) Context.Items[HubDataType.AttemptNumber];
      var hiddenWord = (string) Context.Items[HubDataType.HiddenWord];
      var lastSuggestedLetterNumber = (int) Context.Items[HubDataType.LastSuggestedLetterNumber];
      var getAnswerData = new GetAnswerData(cleanGuessedWord, attemptNumber, hiddenWord, lastSuggestedLetterNumber);
      var answer = GetAnswer(getAnswerData, out var shouldUpdateLastSuggestedLetterNumber,
        out var shouldRestartGame);
      
      UpdateState(shouldUpdateLastSuggestedLetterNumber, lastSuggestedLetterNumber, shouldRestartGame);
      
      await Clients.All.SendAsync("ReceiveMessage", "Я (не Вы)", answer);
    }

    private void UpdateState(bool shouldUpdateLastSuggestedLetterNumber, int lastSuggestedLetterNumber,
      bool shouldRestartGame)
    {
      if (shouldRestartGame)
      {
        InitGame();
      }
      else
      {
        if (shouldUpdateLastSuggestedLetterNumber)
          Context.Items[HubDataType.LastSuggestedLetterNumber] = lastSuggestedLetterNumber + 1;

        Context.Items[HubDataType.AttemptNumber] = (int)Context.Items[HubDataType.AttemptNumber] + 1;
      }
    }

    private void TryInitGame()
    {
      if (!Context.Items.ContainsKey(HubDataType.HiddenWord))
      {
        InitGame();
      }
    }

    private void InitGame()
    {
      var randomWord = GetRandomElement(Words);

      Context.Items[HubDataType.HiddenWord] = randomWord;
      Context.Items[HubDataType.AttemptNumber] = 1;
      Context.Items[HubDataType.LastSuggestedLetterNumber] = 0;
    }

    private static string GetAnswer(GetAnswerData getAnswerData, out bool shouldUpdateLastSuggestedLetterNumber,
      out bool shouldRestartGame)
    {
      shouldRestartGame = false;
      if (getAnswerData.HiddenWord == getAnswerData.GuessedWord)
      {
        shouldUpdateLastSuggestedLetterNumber = false;
        shouldRestartGame = true;
        return "Верно! Играем ещё раз!";
      }

      if (getAnswerData.AttemptNumber % 2 == 0)
      {
        return GetHint(getAnswerData.GuessedWord, getAnswerData.HiddenWord, getAnswerData.LastSuggestedLetterNumber, 
          out shouldUpdateLastSuggestedLetterNumber);
      }
      else if (getAnswerData.AttemptNumber % 3 == 0)
      {
        shouldUpdateLastSuggestedLetterNumber = false;
        return GetRandomElement(HumiliatingAnswers);
      }
      else
      {
        shouldUpdateLastSuggestedLetterNumber = false;
        return GetRandomElement(SimpleAnswers);
      }
    }

    private static T GetRandomElement<T>(List<T> list)
    {
      var randomIndex = RandomGenerator.Next(list.Count);
      return list[randomIndex];
    }

    private static string GetHint(string guessedWord, string hiddenWord, int lastSuggestedLetterNumber,
      out bool shouldUpdateLastSuggestedLetterNumber)
    {
      shouldUpdateLastSuggestedLetterNumber = false;
      if (guessedWord.Length != hiddenWord.Length)
        return GetLengthHint(guessedWord, hiddenWord);

      return GetLetterHint(hiddenWord, lastSuggestedLetterNumber, ref shouldUpdateLastSuggestedLetterNumber);
    }

    private static string GetLengthHint(string guessedWord, string hiddenWord)
    {
      if (guessedWord.Length < hiddenWord.Length)
        return "У меня длиннее";
      else
        return "Загаданное слово короче";
    }

    private static string GetLetterHint(string hiddenWord, int lastSuggestedLetterNumber,
      ref bool shouldUpdateLastSuggestedLetterNumber)
    {
      if (lastSuggestedLetterNumber == hiddenWord.Length)
        return "Все буквы у тебя в руках!";

      shouldUpdateLastSuggestedLetterNumber = true;
      var nextSuggestedNumber = lastSuggestedLetterNumber + 1;
      var nextSuggestedIndex = nextSuggestedNumber - 1;
      return $"Буква номер {nextSuggestedNumber}: {hiddenWord[nextSuggestedIndex]}";
    }
  }
  
  public enum HubDataType
  {
    HiddenWord,
    AttemptNumber,
    LastSuggestedLetterNumber
  }
}
