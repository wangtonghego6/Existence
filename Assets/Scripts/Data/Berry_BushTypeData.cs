using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class Berry_BushTypeData: IMapObjectTypeData
{
    public int lastPickUpDayNum = -1; // 浆果最后一次被采摘的天数
}
