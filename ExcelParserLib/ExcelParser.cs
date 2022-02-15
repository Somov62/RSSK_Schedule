using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drawing = System.Drawing;

namespace ExcelParserLib
{
    public class ExcelParser
    {
        private string _pathToExcel;
        private string _fillColorHex = string.Empty;
        private Drawing.Color _fillColor;
        private DateTime _currentDate;

        public ExcelParser(string pathToExcel)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            CellFillColor = Drawing.Color.FromArgb(255, 153, 204, 255);
            PathToExcel = pathToExcel;
        }
        public string PathToExcel 
        {
            get => _pathToExcel;
            set
            {
                if (!File.Exists(value))
                {
                    throw new ArgumentException("Excel file not found");
                }
                _pathToExcel = value;
            }
        }
        public Drawing.Color CellFillColor
        {
            get => _fillColor;
            set
            {
                _fillColor = value;
                _fillColorHex = HexConverter(value);
            }
        }

        public string GetSchedule(string groupName, DateTime currentDate)
        {
            _currentDate = currentDate;
            if (_currentDate.DayOfWeek == DayOfWeek.Sunday) _currentDate = _currentDate.AddDays(1);
            var file = new FileInfo(PathToExcel);
            using (var package = new ExcelPackage(file))
            {
                var sheet = package.Workbook.Worksheets.First();
                int i = GetGroupColumnIndex(sheet, groupName);
                int widthGroupTable = GetGroupColumnWidth(sheet, i);

                string raspisanie = string.Empty;
                int localI = 6;
                int localJ = i;
                DayOfWeek currentDayOfWeek = _currentDate.DayOfWeek;
                switch (currentDayOfWeek)
                {
                    case DayOfWeek.Monday:
                        localI = 6;
                        localJ = i;
                        raspisanie = "Расписание на понедельник для группы " + groupName + "\n";
                        break;
                    case DayOfWeek.Tuesday:
                        localI = 18;
                        localJ = i;
                        raspisanie = "Расписание на вторник для группы " + groupName + "\n";
                        break;
                    case DayOfWeek.Wednesday:
                        localI = 30;
                        localJ = i;
                        raspisanie = "Расписание на среду для группы " + groupName + "\n";
                        break;
                    case DayOfWeek.Thursday:
                        localI = 42;
                        localJ = i;
                        raspisanie = "Расписание на четверг для группы " + groupName + "\n";
                        break;
                    case DayOfWeek.Friday:
                        localI = 54;
                        localJ = i;
                        raspisanie = "Расписание на пятницу для группы " + groupName + "\n";
                        break;
                    case DayOfWeek.Saturday:
                        localI = 66;
                        localJ = i;
                        raspisanie = "Расписание на субботу для группы " + groupName + "\n";
                        break;
                }

                if (widthGroupTable == 2)
                {
                    raspisanie += GetRasp2Columns(sheet, localI, localJ);
                }
                if (widthGroupTable > 2)
                {
                    raspisanie += GetRasp3Columns(sheet, widthGroupTable, localI, localJ);
                }
                return raspisanie;
            }
        }

        private int GetGroupColumnIndex(ExcelWorksheet sheet, string groupName)
        {
            for (int i = 1; i < 89; i++)
            {
                //Находим столбец с группой
                if (sheet.Cells[4, i].Value?.ToString() == groupName)
                    return i;
            }
            throw new Exception("Group is not founded");
        }

        private int GetGroupColumnWidth(ExcelWorksheet sheet, int startIndex)
        {
            //Находим ширину столбца
            int j = startIndex + 1;
            while (sheet.Cells[4, j + 1].Value is null && j < 88)
            {
                j++;
            }
            j++;
            return j - startIndex;
        }

        private string HexConverter(Drawing.Color c)
        {
            return c.A.ToString("X2") + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }
        private bool IsWeekEven(DateTime currentDay)
        {
            DateTime beginStudyYear;
            if (currentDay < new DateTime(currentDay.Year, 9, 1))
                beginStudyYear = new DateTime(currentDay.Year - 1, 9, 1);
            else beginStudyYear = new DateTime(currentDay.Year, 9, 1);

            int days = (currentDay - beginStudyYear).Days + (int)beginStudyYear.DayOfWeek - 1;

            int week = days / 7 + 1;
            return week % 2 == 0;
        }

        private string GetRasp3Columns(ExcelWorksheet sheet, int widthGroupTable, int localI, int localJ)
        {
            ExcelRange daySchedule = sheet.Cells[localI, localJ, localI + 11, localJ + widthGroupTable];
            StringBuilder rasp = new StringBuilder();
            for (int lessons = localI; lessons < localI + 11; lessons += 2) //парсим по парам 0-1-2-3-4-5
            {
                string lesson = $"{(lessons - localI) / 2} - ";
                if (!(daySchedule[lessons + 1, localJ + 1].Value is null) && !daySchedule[lessons + 1, localJ + 1].Style.Font.Italic)
                {
                    if (IsWeekEven(_currentDate))
                    {
                        lesson += daySchedule[lessons + 1, localJ].Value?.ToString()
                        + daySchedule[lessons + 1, localJ].Style.Fill.BackgroundColor.Rgb == _fillColorHex ? " Синий" : ""
                        + " (Подгруппа А)";
                        lesson += daySchedule[lessons + 1, localJ + 1].Value?.ToString()
                        + daySchedule[lessons + 1, localJ + 1].Style.Fill.BackgroundColor.Rgb == _fillColorHex ? " Синий" : ""
                        + " (Подгруппа Б)";
                        lesson += "   нижняя неделя";
                        rasp.AppendLine(lesson);
                        continue;
                    }
                }

                if (!(daySchedule[lessons, localJ + 1].Value is null) && daySchedule[lessons, localJ + 2] is null)
                {
                    if (!IsWeekEven(_currentDate))
                    {
                        lesson += daySchedule[lessons, localJ].Value?.ToString()
                        + daySchedule[lessons, localJ].Style.Fill.BackgroundColor.Rgb == _fillColorHex ? "Синий" : ""
                        + " (Подгруппа А)";
                        lesson += daySchedule[lessons, localJ + 1].Value?.ToString()
                        + daySchedule[lessons, localJ + 1].Style.Fill.BackgroundColor.Rgb == _fillColorHex ? "Синий" : ""
                        + " (Подгруппа Б)";
                        lesson += "   верхняя неделя";
                        rasp.AppendLine(lesson);
                        continue;
                    }

                }

                if (!(daySchedule[lessons + 1, localJ + 1].Value is null) && daySchedule[lessons + 1, localJ + 1].Style.Font.Italic)
                {
                    lesson += daySchedule[lessons, localJ].Value?.ToString() + " " + daySchedule[lessons + 1, localJ].Value?.ToString();
                    lesson += daySchedule[lessons, localJ].Style.Fill.BackgroundColor.Rgb == _fillColorHex ? " Синий" : "";
                    lesson += " (Подгруппа А)\n    ";
                    lesson += daySchedule[lessons, localJ + 1].Value?.ToString() + " " + daySchedule[lessons + 1, localJ + 1].Value?.ToString();
                    lesson += daySchedule[lessons, localJ + 1].Style.Fill.BackgroundColor.Rgb == _fillColorHex ? " Синий" : "";
                    lesson += " (Подгруппа Б)";
                    rasp.AppendLine(lesson);
                    continue;
                }



                if (daySchedule[lessons + 1, localJ]?.Style.Font.Italic == true && daySchedule[lessons, localJ + 1].Value is null && !(daySchedule[lessons, localJ].Value is null) && !(daySchedule[lessons + 1, localJ].Value is null))//пара без недельного разделения
                {
                    lesson += daySchedule[lessons, localJ].Value?.ToString();

                    lesson += " | " + daySchedule[lessons + 1, localJ].Value?.ToString();

                    if (!(daySchedule[lessons, localJ + 2].Value is null)) lesson += " | Кабинет-";
                    lesson += daySchedule[lessons, localJ + 2].Value?.ToString() + "," + daySchedule[lessons + 1, localJ + 2].Value?.ToString();
                    if (lesson == $"{(lessons - localI) / 2} -  | ") continue;

                    if (daySchedule[lessons, localJ].Style.Fill.BackgroundColor.Rgb == _fillColorHex)
                        lesson += " Синий";
                    rasp.AppendLine(lesson);
                    continue;
                }
                //Обрабатываем пары с недельным разделением 
                if (!IsWeekEven(_currentDate))
                {
                    if (daySchedule[lessons, localJ].Value is null) continue;
                    lesson += daySchedule[lessons, localJ].Value?.ToString();
                    if (!(daySchedule[lessons, localJ + 2].Value is null))
                        lesson += " | Кабинет-";
                    lesson += daySchedule[lessons, localJ + 2].Value?.ToString();
                    if (daySchedule[lessons, localJ].Style.Fill.BackgroundColor.Rgb == _fillColorHex)
                        lesson += " Синий";
                    lesson += "   верхняя неделя";
                    rasp.AppendLine(lesson);
                    continue;
                }
                if (daySchedule[lessons + 1, localJ].Value is null) continue;
                lesson += daySchedule[lessons + 1, localJ].Value?.ToString();
                if (!(daySchedule[lessons + 1, localJ + 2].Value is null)) lesson += " | Кабинет-";
                lesson += daySchedule[lessons + 1, localJ + 2].Value?.ToString();
                if (daySchedule[lessons + 1, localJ].Style.Fill.BackgroundColor.Rgb == _fillColorHex)
                    lesson += " Синий";
                lesson += "   нижняя неделя";
                rasp.AppendLine(lesson);
            }

            return rasp.ToString();
        }

        private string GetRasp2Columns(ExcelWorksheet sheet, int localI, int localJ)
        {
            ExcelRange daySchedule = sheet.Cells[localI, localJ, localI + 11, localJ + 2];
            StringBuilder rasp = new StringBuilder();
            for (int lessons = localI; lessons < localI + 11; lessons += 2) //парсим по парам 0-1-2-3-4-5
            {
                string lesson = $"{(lessons - localI) / 2} - ";
                if (daySchedule[lessons + 1, localJ]?.Style.Font.Italic == true && !(daySchedule[lessons, localJ].Value is null) && !(daySchedule[lessons + 1, localJ].Value is null))//пара без недельного разделения
                {
                    lesson += daySchedule[lessons, localJ].Value?.ToString();

                    lesson += " | " + daySchedule[lessons + 1, localJ].Value?.ToString();

                    if (!(daySchedule[lessons, localJ + 1].Value is null)) lesson += " | Кабинет-";
                    lesson += daySchedule[lessons, localJ + 1].Value?.ToString();
                    if (lesson == $"{(lessons - localI) / 2} -  | ") continue;

                    if (daySchedule[lessons, localJ].Style.Fill.BackgroundColor.Rgb == _fillColorHex)
                        lesson += " Синий";
                    rasp.AppendLine(lesson);
                    continue;
                }
                //Обрабатываем пары с недельным разделением 
                if (!IsWeekEven(_currentDate))
                {
                    if (daySchedule[lessons, localJ].Value is null) continue;
                    lesson += daySchedule[lessons, localJ].Value?.ToString();
                    if (!(daySchedule[lessons, localJ + 1].Value is null))
                        lesson += " Кабинет-";
                    lesson += daySchedule[lessons, localJ + 1].Value?.ToString();
                    if (daySchedule[lessons, localJ].Style.Fill.BackgroundColor.Rgb == _fillColorHex)
                        lesson += " Синий";
                    lesson += "   верхняя неделя";
                    rasp.AppendLine(lesson);
                    continue;
                }
                if (daySchedule[lessons + 1, localJ].Value is null) continue;
                lesson += daySchedule[lessons + 1, localJ].Value?.ToString();
                if (!(daySchedule[lessons + 1, localJ + 1].Value is null)) lesson += " Кабинет-";
                lesson += daySchedule[lessons + 1, localJ + 1].Value?.ToString();
                if (daySchedule[lessons + 1, localJ].Style.Fill.BackgroundColor.Rgb == _fillColorHex)
                    lesson += " Синий";
                lesson += "   нижняя неделя";
                rasp.AppendLine(lesson);
            }
            return rasp.ToString();
        }
    }
}
