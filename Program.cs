using System;
using System.IO;
using OfficeOpenXml;
// Pet 클래스
public abstract class Pet
{
    public string Pmanage_no { get; set; } // 관리번호
    public string Pname { get; set; } // 이름
    public int Page { get; set; } // 나이
    public string Pbreed { get; set; } // 품종
    public string PState { get; set; } // 상태
    public int Psrv_type { get; set; } // 서비스 유형
 
    public Pet(string manageNo, string name, int age, string breed, string state, int srvType)
    {
        Pmanage_no = manageNo;
        Pname = name;
        Page = age;
        Pbreed = breed;
        PState = state;
        Psrv_type = srvType;
    }

    public abstract void PrintInfo();

    public static string Decode_Srv_type(int s_type)
    {
        return s_type switch
        {
            1 => "[목욕]",
            2 => "[커트]",
            3 => "[목욕,커트]",
            4 => "[발관리]",
            5 => "[목욕,발관리]",
            6 => "[커트,발관리]",
            7 => "[목욕,커트,발관리]",
            _ => "<요청코드오류>",
        };
    }
}

public class PetCat : Pet
{
    public PetCat(string manageNo, string name, int age, string breed, string state, int srvType)
        : base(manageNo, name, age, breed, state, srvType) { }

    public override void PrintInfo()
    {
        Console.WriteLine($"고양이 - 관리번호: {Pmanage_no}, 이름: {Pname}, 나이: {Page}, 품종: {Pbreed}, 상태: {PState}, 서비스 유형: {Decode_Srv_type(Psrv_type)}");
    }
}

public class PetDog : Pet
{
    public int Psize { get; set; } // 견 크기

    public PetDog(string manageNo, string name, int age, string breed, string state, int srvType, int size)
        : base(manageNo, name, age, breed, state, srvType)
    {
        Psize = size;
    }

    public override void PrintInfo()
    {
        Console.WriteLine($"강아지 - 관리번호: {Pmanage_no}, 이름: {Pname}, 나이: {Page}, 품종: {Pbreed}, 상태: {PState}, 서비스 유형: {Decode_Srv_type(Psrv_type)}, 크기: {Psize}");
    }
}
// ExcelFileHandler 클래스
public class ExcelFileHandler
{
    private string filePath;

    public ExcelFileHandler(string filePath)
    {
        this.filePath = filePath;
    }
// 엑셀 파일로부터 데이터 읽기
    public string[][] ReadDataFromExcel(int sheetNo)
    {
        try
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[sheetNo];// 첫 번째 시트 선택
                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;

                string[][] data = new string[rowCount][];

                for (int row = 1; row <= rowCount; row++)
                {
                    data[row - 1] = new string[colCount];
                    for (int col = 1; col <= colCount; col++)
                    {
                        data[row - 1][col - 1] = worksheet.Cells[row, col].Value?.ToString() ?? "";
                    }
                }

                return data;
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"엑셀 파일 데이터 읽기 오류: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"일반 오류: {ex.Message}");
            return null;
        }
    }
}
// PetManagementSystem 클래스
public class PetManagementSystem
{
    const int MaxPet = 10;
    public int Ccount = 0; // 현재 고양이 관리 마리수
    public int Dcount = 0; // 현재 강아지 관리 마리수
    public Pet[] pets = new Pet[MaxPet * 2]; // 고양이와 강아지를 하나의 배열로 관리
    public CostType[] CostData = new CostType[4]; // 코스트데이터 배열로 관리

    private string petEntryFilePath;
    private string petManagedFilePath;

    public PetManagementSystem(string petEntryFilePath, string petManagedFilePath)
    {
        this.petEntryFilePath = petEntryFilePath;
        this.petManagedFilePath = petManagedFilePath;
    }

    public void Run()
    {
        ExcelFileHandler fileHandler = new ExcelFileHandler(petManagedFilePath);

        try
        {
            string[] lines = File.ReadAllLines(petEntryFilePath);

            Console.WriteLine($"{petEntryFilePath} 파일로부터 펫 입고 정보 읽기 확인:");
            foreach (string line in lines)
            {
                Console.WriteLine(line);
                string[] fields = line.Split(',');
                string actCode = fields[0].Trim(); // D:개 데이터, C:고양이 데이터
                if (actCode == "C")
                {
                    AddPet(new PetCat(fields[1].Trim(), fields[2].Trim(), int.Parse(fields[3].Trim()), fields[4].Trim(), fields[5].Trim(), int.Parse(fields[6].Trim())));
                }
                else if (actCode == "D")
                {
                    AddPet(new PetDog(fields[1].Trim(), fields[2].Trim(), int.Parse(fields[3].Trim()), fields[4].Trim(), fields[5].Trim(), int.Parse(fields[6].Trim()), int.Parse(fields[7].Trim())));
                }
            }

            Ccount = Array.FindAll(pets, p => p is PetCat).Length;
            Dcount = Array.FindAll(pets, p => p is PetDog).Length;
            // 엑셀파일로부터 관리상태정보 입력
            Console.WriteLine();
            Console.WriteLine($"{petManagedFilePath} 파일로부터 엑셀 데이터 읽기 확인:");
            string[][] excelData = fileHandler.ReadDataFromExcel(0);
            if (excelData != null)
            {
                foreach (var row in excelData)
                {
                    Manage_PetShop(row);
                    Console.WriteLine(string.Join(", ", row));
                }
            }
            // 엑셀파일로부터 코스트정보 입력
            Console.WriteLine();
            Console.WriteLine($"{petManagedFilePath} 파일로부터 엑셀 데이터 읽기 확인:");
            string[][] excelCostData = fileHandler.ReadDataFromExcel(1);
            if (excelCostData != null)
            {
                int index = 0;
                foreach (var row in excelCostData)
                {
                    Cost_PetShop(index, row);
                    Console.WriteLine(string.Join(", ", row));
                    index++;
                }
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"파일 읽기 오류: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"일반 오류: {ex.Message}");
        }

        PrintManagementInfo();
        PrintTotalCost(); // 총 비용 출력
    }
    // 펫 추가 메서드 (오버로딩)
    public void AddPet(PetCat pet)
    {
        if (Ccount < MaxPet)
        {
            pets[Ccount] = pet;
            Ccount++;
        }
    }

    public void AddPet(PetDog pet)
    {
        if (Dcount < MaxPet)
        {
            pets[MaxPet + Dcount] = pet;
            Dcount++;
        }
    }
    // 펫 관리상태 처리
    public void Manage_PetShop(string[] fields)
    {
        string srch_man_no = fields[2].Trim();
        Pet pet = Array.Find(pets, p => p != null && p.Pmanage_no == srch_man_no);
        if (pet != null)
        {
            pet.PState = fields[3].Trim();
        }
        else
        {
            Console.WriteLine($"관리번호 오류: {srch_man_no}");
        }
    }
    // 펫 관리상태 처리
    public void Cost_PetShop(int index, string[] fields)
    {
        if (index < CostData.Length)
        {
            CostData[index].scode = fields[0].Trim();
            CostData[index].cost = float.Parse(fields[1].Trim());
        }
    }
    // 펫 관리상태 출력
    public void PrintManagementInfo()
    {
        Console.WriteLine("고양이 관리상태 출력");
        Console.WriteLine("===============================");
        foreach (var pet in pets)
        {
            if (pet is PetCat)
            {
                pet.PrintInfo();
            }
        }

        Console.WriteLine("\n\n강아지 관리상태 출력");
        Console.WriteLine("===============================");
        foreach (var pet in pets)
        {
            if (pet is PetDog)
            {
                pet.PrintInfo();
            }
        }
    }
    // 총 비용 출력
    public void PrintTotalCost()
    {
        Console.WriteLine("\n\n총 관리 비용 계산");
        Console.WriteLine("===============================");
        float totalCost = 0;
        foreach (var pet in pets)
        {
            if (pet != null && pet.PState.EndsWith("E"))
            {
                float cost = EvalCost(pet.PState, CostData);
                Console.WriteLine($"관리번호: {pet.Pmanage_no}, 이름: {pet.Pname}, 상태: {pet.PState}, 비용: {cost}원");
                totalCost += cost;
            }
        }
        Console.WriteLine($"\n총 관리 비용: {totalCost}원");
    }

    static string Decode_PetMan_State(string s_type)
    {
        string result = "";
        foreach (char c in s_type)
        {
            result += c switch
            {
                'C' => "[케이지대기중]",
                'F' => "[합사장대기중]",
                'B' => "[목욕완료]",
                'H' => "[커트완료]",
                'T' => "[발톱관리완료]",
                'E' => "<[관리완료]>",
                _ => "<코드오류>",
            };
        }
        return result;
    }

    static float EvalCost(string s_type, CostType[] costdata)
    {
        float cost = 0;
        int ccount = 0;
        foreach (char c in s_type)
        {
            switch (c)
            {
                case 'B':
                    cost += costdata[0].cost;
                    ccount++;
                    break;
                case 'H':
                    cost += costdata[1].cost;
                    ccount++;
                    break;
                case 'T':
                    cost += costdata[2].cost;
                    ccount++;
                    break;
                case 'E':
                    if (ccount == 3)
                        cost *= costdata[3].cost; // 할인 적용
                    break;
            }
        }
        return cost;
    }
}

public struct CostType
{
    public string scode;
    public float cost;
}

public class Program
{
    public static void Main()
    {
        string petEntryFilePath = "petEntry.txt";
        string petManagedFilePath = "petManaged.xlsx";
        PetManagementSystem system = new PetManagementSystem(petEntryFilePath, petManagedFilePath);
        system.Run();
    }
}
