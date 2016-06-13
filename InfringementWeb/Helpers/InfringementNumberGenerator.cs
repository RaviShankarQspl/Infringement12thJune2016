using System;
using System.Linq;

namespace InfringementWeb.Helpers
{
    public static class InfringementNumberGenerator
    {
        private static infringementEntities _entities = new infringementEntities();

        public static string Generate(DateTime infringementTime, string officerCode)
        {
            string yearCode = (infringementTime.Year - 2000).ToString();
            string monthCode = infringementTime.Month.ToString();
            string dayCode = infringementTime.Day.ToString();
            string hourCode = infringementTime.Hour.ToString();
            string minCode = infringementTime.Minute == 0 ? "00" : infringementTime.Minute.ToString();

            var codes = _entities.infringement_number_encryption
                        .Where(x =>
                            x.Value == yearCode ||
                            x.Value == monthCode ||
                            x.Value == dayCode ||
                            x.Value == hourCode ||
                            x.Value == minCode ||
                            x.Value == officerCode
                        ).Select(x => new { Key = x.ShortKey, Value = x.Value })
                        .ToList();

            if (codes.FirstOrDefault(x => x.Value == officerCode) == null)
                throw new InvalidOfficerCodeException();
            var infringementNumber = String.Format("{0}{1}{2}{3}{4}{5}",
                codes.FirstOrDefault(x => x.Value == yearCode).Key,
                codes.FirstOrDefault(x => x.Value == monthCode).Key,
                codes.FirstOrDefault(x => x.Value == dayCode).Key,
                codes.FirstOrDefault(x => x.Value == hourCode).Key,
                codes.FirstOrDefault(x => x.Value == minCode).Key,
                codes.FirstOrDefault(x => x.Value == officerCode).Key
                );

            return infringementNumber;

        }
    }
}