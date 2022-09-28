using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
/// <summary>
/// 科技数据的存档
/// </summary>
public class ScienceData
{
    public List<int> UnlockScienceList = new List<int>(10);

    /// <summary>
    /// 检测解锁
    /// </summary>
    public bool CheckUnLock(int ID)
    { 
        return UnlockScienceList.Contains(ID);
    }

    /// <summary>
    /// 添加科技
    /// </summary>
    public void AddScience(int ID)
    {
        if (!UnlockScienceList.Contains(ID))
        {
            UnlockScienceList.Add(ID);
        }
    }

}