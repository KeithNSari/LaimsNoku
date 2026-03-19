namespace LAIMS.Utilities
{
    public static class Calculations
    {
        public static int CalculateAgeAtNextBirthday(DateTime Birthdate, DateTime CurrentDate)
        {
            int year = CurrentDate.Year;

            // Check if the birthday for this year has already occurred
            DateTime nextBirthday;

            // Handling February 29th in a non-leap year
            if (Birthdate.Month == 2 && Birthdate.Day == 29 && !DateTime.IsLeapYear(year))
            {
                nextBirthday = new DateTime(year, 2, 28);
            }
            else
            {
                nextBirthday = new DateTime(year, Birthdate.Month, Birthdate.Day);
            }

            if (CurrentDate > nextBirthday)
            {
                // If the birthday has passed for this year, calculate for the next year
                nextBirthday = nextBirthday.AddYears(1);
            }

            // Calculate the age at the next birthday
            int ageAtNextBirthday = nextBirthday.Year - Birthdate.Year;

            return ageAtNextBirthday;
        }
        public static int CalculateCurrentAge(DateTime birthdate, DateTime currentDate)
        {
            int age = currentDate.Year - birthdate.Year;

            // Check if the birthday for this year has not occurred yet
            if (birthdate.Month > currentDate.Month || (birthdate.Month == currentDate.Month && birthdate.Day > currentDate.Day))
            {
                age--;
            }

            return age;
        }
        public static int GenerateRandomNumber(int minValue, int maxValue)
        {  
            Random random = new Random(); 
            int randomNumber = random.Next(minValue, maxValue + 1);
            return randomNumber; 
        }
    }
}
