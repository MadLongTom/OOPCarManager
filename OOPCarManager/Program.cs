// See https://aka.ms/new-console-template for more information

PersonalInfo personalInfo = new("myname", 999999999, false, new DateTime(2004, 1, 1), "looks like my Id");
string input;
int recordNum;
Console.WriteLine("请输入要记录的个数");
do
{
    input = Console.ReadLine()!;
} while (!int.TryParse(input, out recordNum));
// 使用循环获取用户输入的记录个数，直到输入的内容可以成功转换为整数
List<Type> types = [.. typeof(Car).Assembly.GetTypes().Where(t => t.BaseType == typeof(Car))];
// 获取所有继承自Car类的类型
List<Record> instances = [];
for (int i = 0; i < recordNum; i++)
{
    Console.WriteLine($"开始记录{i + 1}");
    Console.WriteLine($"请输入日期，输入回车则使用当前时间（格式：{DateTime.Now.ToShortDateString()}）：");
    DateTime? dt;
    loc_input:
    input = Console.ReadLine()!;
    if (input != string.Empty)
    {
        try
        {
            dt = Convert.ToDateTime(input);
        }
        catch (Exception)
        {
            Console.WriteLine("输入有误，请重新输入:");
            goto loc_input;
        }
    }
    else
    {
        dt = DateTime.Now;
    }
    // 使用循环获取用户输入的日期，直到输入的内容可以成功转换为DateTime类型，或者为空（使用当前时间）
    Console.WriteLine($"请输入车辆类型：{string.Join(',', types.Select(t => $"{types.IndexOf(t)} - {t.Name}"))}");
    int selection;
    do
    {
        input = Console.ReadLine()!;
    } while (!(int.TryParse(input, out selection) && Enumerable.Range(0, types.Count).Contains(selection)));
    // 使用循环获取用户输入的车辆类型选择，直到输入的内容可以成功转换为整数且在可选范围内
    Type type = types[selection];
    List<object> parameters = [];
    foreach (var parameterInfo in type.GetConstructors().First().GetParameters())//查询对应类的构造函数，要求用户输入参数
    {
        if (parameterInfo.Name == "UserAge")
        {
            int years = (dt - personalInfo.BirthDay).Value.Days / 365;
            // 根据出生日期计算年龄
            if (!ManagerHelper.VerifyAge(type, years))
            {
                Console.WriteLine("年龄不在合法范围内，本次记录中断。");
                goto loc_continue;
            }
            // 判断是否为UserAge参数，如果是，计算年龄并验证是否在合法范围内
            parameters.Add(years);
            continue;
        }
        Console.Write($"{parameterInfo.Name}:");
        if (parameterInfo.ParameterType == typeof(int))
        {
            if (parameterInfo.Name == "Size" && type.Name == "Bike")
            {
                Console.Write($"（合法的尺寸有{string.Join(',', ManagerHelper.LegalSize)}）");
                // 如果参数类型为int，并且是Bike类型的Size参数，打印合法尺寸提示信息
            }
            int outNum;
            do
            {
                input = Console.ReadLine()!;
            } while (!int.TryParse(input, out outNum));
            // 使用循环获取用户输入的整数
            if (parameterInfo.Name == "Size" && type.Name == "Bike")
            {
                if (!ManagerHelper.LegalSize.Contains(outNum))
                {
                    Console.WriteLine("尺寸不在合法范围内，本次记录中断。");
                    goto loc_continue;
                }
            }
            // 如果是Bike类型的Size参数，并且输入的尺寸不在合法范围内，中断本次记录
            parameters.Add(outNum);
        }
        else
        {
            // 如果参数类型不是int，直接获取用户输入的字符串
            input = Console.ReadLine()!;
            parameters.Add(input);
        }
    }
    // 根据用户输入的信息创建记录实例，并添加到列表中
    instances.Add(new Record(dt.Value, Activator.CreateInstance(type, [.. parameters]) as Car));
    loc_continue:;
}
Console.WriteLine("打印结果");
Console.WriteLine(personalInfo);
Console.WriteLine("本次记录结果如下：");
foreach (var instance in instances)
{
    Console.WriteLine(instance + Environment.NewLine);
}

public record PersonalInfo(string Name, long Serial, bool IsFemale, DateTime BirthDay, string Id);
// 个人信息记录，包括姓名、序列号、性别、出生日期和身份证号码

public record Record(DateTime RecordDate, Car? RecordCar)
{
    public override string ToString() => $"RecordDate={RecordDate.ToShortDateString()}{Environment.NewLine}{RecordCar}";
}
// 记录类，包括记录日期和记录的车辆对象，重写ToString方法用于打印记录信息

public static class ManagerHelper
{
    private static readonly Dictionary<string, string> DataUnits = new()
    {
        { "MaxSpeed","km/h" },
        { "UserAge","Years Old" },
        { "Size","Inch"}
    };// 数据单位字典，用于存储不同数据字段对应的单位
    public static string GetUnit(string key) => DataUnits.ContainsKey(key) ? DataUnits[key] : string.Empty;
    // 根据字段名获取数据单位的方法
    private static readonly Dictionary<Type, Func<int, bool>> Verifications = new()
    {
        {typeof(ToyCar),userAge => userAge < 8 },
        {typeof(Bike),userAge => userAge > 8 },
        {typeof(ElectricBike),userAge => userAge > 18 },
        {typeof(AutoMobile),userAge => userAge > 23 },
    };// 年龄验证委托字典，用于存储不同车辆类型对应的年龄验证委托方法
    public static bool VerifyAge(Type carType, int userAge) => Verifications.ContainsKey(carType) && Verifications[carType](userAge);
    // 验证年龄是否符合车辆类型要求的方法
    public static readonly List<int> LegalSize = [20, 22, 24, 28];// 合法尺寸列表，用于存储合法的车辆尺寸选项
}

public abstract record Car(string Owner, int UserAge)
{
    public abstract double MaxSpeed { get; }
    public override string ToString() =>
        string.Join(Environment.NewLine,
            [$"VehicleType:{GetType().Name}",
                string.Join(Environment.NewLine, GetType().GetFields()
                .Select(f => $"{f.Name}:{f.GetValue(this)} {ManagerHelper.GetUnit(f.Name)}")),
                string.Join(Environment.NewLine, GetType().GetProperties()
                .Select(p => $"{p.Name}:{p.GetValue(this)} {ManagerHelper.GetUnit(p.Name)}"))]);
    // 车辆信息的ToString方法，包括车辆类型、字段和属性的名称、值和单位的打印
}

public sealed record AutoMobile(string Owner, int UserAge, int MaxPassenger) : Car(Owner, UserAge)
{
    public override double MaxSpeed => Math.Min(120, 40 + 40 * (UserAge - 23));
    public override string ToString() => base.ToString();
}

public sealed record ElectricBike(string Owner, int UserAge, int MaxPassenger, int Size) : Car(Owner, UserAge)
{
    public override double MaxSpeed => Math.Min(60, 20 + 10 * (UserAge - 18));
    public override string ToString() => base.ToString();
}

public sealed record Bike(string Owner, int UserAge, int Size) : Car(Owner, UserAge)
{
    public override double MaxSpeed => Math.Min(15, 10 + 0.5 * (UserAge - 8));
    public override string ToString() => base.ToString();
}

public sealed record ToyCar(string Owner, int UserAge, string Material) : Car(Owner, UserAge)
{
    public override double MaxSpeed => 0.2 * UserAge;
    public override string ToString() => base.ToString();
}