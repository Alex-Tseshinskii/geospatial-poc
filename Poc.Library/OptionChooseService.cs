using System.ComponentModel;
using Poc.Model;

namespace Poc.Library;

public static class OptionChooseService
{
    public static RunOption ChooseOption()
    {
        RunOption option;
        bool validInput;

        Console.WriteLine("Please choose run option:");
        foreach(var runOption in Enum.GetValues<RunOption>())
        {
            var description = GetEnumDescription(runOption);
            Console.WriteLine($"{(int)runOption}: {description}");
        }

        do
        {
            Console.Write("Your option: ");
            validInput = ValidateInput(Console.ReadLine(), out option);

            if (!validInput)
            {
                Console.WriteLine("Invalid input, please choose valid option:");
            }
        } while (!validInput);

        return option;
    }

    private static string GetEnumDescription<T>(T enumValue)
        where T : struct, Enum
    {
        var type = enumValue.GetType();
        var fieldInfo = type.GetField(enumValue.ToString()) ?? throw new InvalidOperationException();
        var attribute = fieldInfo
            .GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;

        return attribute?.Description ?? enumValue.ToString();
    }

    private static bool ValidateInput(string? input, out RunOption option)
    {
        return Enum.TryParse(input, out option) &&
                     Enum.IsDefined(typeof(RunOption), option);
    }

    public static bool ChooseIndexCreation()
    {
        Console.Write("Create index? (y/n): ");
        bool? userChoice;
        do
        {
            userChoice = Console.ReadLine()?.ToLowerInvariant() switch
            {
                "y" => true,
                "n" => false,
                _ => null,
            };

            if (userChoice == null)
            {
                Console.WriteLine("Invalid input, please choose valid option:");
            }
        } while (!userChoice.HasValue);

        return userChoice.Value;
    }
}