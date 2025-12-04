using System;
using UnityEngine;
using DamageNumbersPro;

public class EnemyHealth : MonoBehaviour
{
    public static EnemyHealth Instance { get; private set; }

    [Header("普通小怪：单条血（如果不用多条血，就用这一套）")]
    public int maxHP = 50;
    public int currentHP;

    [Header("Boss 多条血条设置")]
    public bool useSegments = false;            // 勾上 = 使用多条血
    public int[] segmentMaxHPs;                 // 每一条血的最大值
    [NonSerialized] public int[] segmentCurrentHPs; // 运行时当前值（自动初始化）

    // 事件：总血量变化（总当前，总最大），和每条血的变化（数组）
    public event Action<int, int> OnHPChanged;                    // totalCurrent, totalMax
    public event Action<int[], int[]> OnSegmentsChanged;          // segmentCurrents, segmentMaxes
    public event Action OnDead;

    [Header("Damage / Heal Numbers")]
    public DamageNumberGUI damageNumberPrefab;   // 伤害数字 prefab
    public DamageNumberGUI healNumberPrefab;     // 治疗数字 prefab（你新建的）
    public RectTransform damageNumberParent;     // 一般就是主 Canvas 下面的一个 RectTransform
    [Tooltip("伤害数字的偏移")]
    public Vector2 damageNumberOffset = new Vector2(0, 60);

    [Tooltip("治疗数字的偏移")]
    public Vector2 healNumberOffset = new Vector2(0, 80);   // ← 新增
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (useSegments && segmentMaxHPs != null && segmentMaxHPs.Length > 0)
        {
            // 初始化每条血的当前值 = 最大值
            segmentCurrentHPs = new int[segmentMaxHPs.Length];
            for (int i = 0; i < segmentMaxHPs.Length; i++)
            {
                segmentCurrentHPs[i] = Mathf.Max(0, segmentMaxHPs[i]);
            }
            NotifyAll();
        }
        else
        {
            // 单条血模式
            currentHP = maxHP;
            NotifyAll();
        }
    }

    /// <summary>
    /// 被攻击扣血。多条血条情况下，伤害会从第 1 条开始依次扣到后面。
    /// 如果所有血条都 <=0，则死亡。
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (amount <= 0)
        {
            Debug.Log("[EnemyHealth] TakeDamage 被调用，但 amount <= 0，直接返回");
            return;
        }

        Debug.Log($"[EnemyHealth] TakeDamage({amount})，useSegments={useSegments}");

        if (useSegments && segmentCurrentHPs != null && segmentCurrentHPs.Length > 0)
        {
            int remaining = amount;
            int totalDealt = 0;

            for (int i = 0; i < segmentCurrentHPs.Length && remaining > 0; i++)
            {
                int cur = segmentCurrentHPs[i];
                if (cur <= 0) continue;

                int newCur = Mathf.Max(0, cur - remaining);
                int dealt = cur - newCur;
                remaining -= dealt;
                segmentCurrentHPs[i] = newCur;
                totalDealt += dealt;
            }

            Debug.Log($"[EnemyHealth] 多条血受到 {amount} 点伤害，实际扣掉 {totalDealt} 点，当前：{GetSegmentsDebug()}");

            // ★ 多条血也显示数字（用 totalDealt）
            if (totalDealt > 0)
            {
                ShowDamageNumber(totalDealt);
            }

            bool allZero = true;
            for (int i = 0; i < segmentCurrentHPs.Length; i++)
            {
                if (segmentCurrentHPs[i] > 0)
                {
                    allZero = false;
                    break;
                }
            }

            NotifyAll();

            if (allZero)
            {
                Debug.Log("[EnemyHealth] 敌人所有血条被打空，死亡！");
                OnDead?.Invoke();
            }
        }
        else
        {
            // 单条血逻辑
            currentHP -= amount;
            if (currentHP < 0) currentHP = 0;

            Debug.Log($"[EnemyHealth] 单条血受到 {amount} 点伤害，当前 HP：{currentHP}/{maxHP}");

            ShowDamageNumber(amount);

            NotifyAll();

            if (currentHP <= 0)
            {
                Debug.Log("[EnemyHealth] 敌人死亡（单条血）");
                OnDead?.Invoke();
            }
        }
    }

    /// <summary>
    /// 敌人回复生命（正值 = 回复）
    /// </summary>
    public void Heal(int amount)
    {
        if (amount <= 0) return;

        if (useSegments && segmentCurrentHPs != null && segmentMaxHPs != null && segmentMaxHPs.Length > 0)
        {
            // 简单示例：把回复量加到第 1 条血上，你之后可以设计更复杂的逻辑
            int i = 0;
            int max = Mathf.Max(0, segmentMaxHPs[0]);
            if (max > 0)
            {
                int old = segmentCurrentHPs[0];
                int newCur = Mathf.Clamp(old + amount, 0, max);
                int healed = newCur - old;
                segmentCurrentHPs[0] = newCur;

                if (healed > 0)
                {
                    ShowHealNumber(healed);
                }
            }

            Debug.Log($"敌人回复 {amount} 点生命，多条血：{GetSegmentsDebug()}");
            NotifyAll();
        }
        else
        {
            int old = currentHP;
            currentHP += amount;
            if (currentHP > maxHP) currentHP = maxHP;

            int healed = currentHP - old;
            if (healed > 0)
            {
                ShowHealNumber(healed);
            }

            Debug.Log($"敌人回复 {amount} 点生命，当前 HP：{currentHP}/{maxHP}");
            NotifyAll();
        }
    }

    /// <summary>
    /// Token：设置某条血的当前 HP（可以把所有条都改成很低，然后一击打空）
    /// </summary>
    public void SetSegmentHPByToken(int index, int value)
    {
        if (!useSegments || segmentCurrentHPs == null || segmentMaxHPs == null)
            return;

        if (index < 0 || index >= segmentCurrentHPs.Length)
            return;

        int max = Mathf.Max(1, segmentMaxHPs[index]); // 至少 1
        int old = segmentCurrentHPs[index];
        segmentCurrentHPs[index] = Mathf.Clamp(value, 0, max);
        int delta = segmentCurrentHPs[index] - old;

        if (delta < 0)
            ShowDamageNumber(-delta);
        else if (delta > 0)
            ShowHealNumber(delta);

        Debug.Log($"[Token] 设置敌人第 {index + 1} 条血为 {segmentCurrentHPs[index]}/{max}");
        NotifyAll();
    }

    /// <summary>
    /// （可选）Token：修改某条血的最大值
    /// </summary>
    public void SetSegmentMaxHPByToken(int index, int value, bool clampCurrent = true)
    {
        if (!useSegments || segmentCurrentHPs == null || segmentMaxHPs == null)
            return;

        if (index < 0 || index >= segmentMaxHPs.Length)
            return;

        segmentMaxHPs[index] = Mathf.Max(1, value);
        if (clampCurrent)
        {
            int old = segmentCurrentHPs[index];
            segmentCurrentHPs[index] = Mathf.Clamp(segmentCurrentHPs[index], 0, segmentMaxHPs[index]);
            int delta = segmentCurrentHPs[index] - old;

            if (delta < 0)
                ShowDamageNumber(-delta);
            else if (delta > 0)
                ShowHealNumber(delta);
        }

        Debug.Log($"[Token] 设置第 {index + 1} 条血的最大值为 {segmentMaxHPs[index]}，当前 {segmentCurrentHPs[index]}");
        NotifyAll();
    }

    /// <summary>
    /// Token：直接设置敌人的“总当前 HP”（单条血用；多条血时自动拆到各条上）
    /// </summary>
    public void SetCurrentHPByToken(int value)
    {
        if (useSegments && segmentCurrentHPs != null && segmentMaxHPs != null)
        {
            // 计算修改前总血量
            int oldTotal = 0;
            for (int i = 0; i < segmentCurrentHPs.Length; i++)
                oldTotal += Mathf.Max(0, segmentCurrentHPs[i]);

            // 多条血：把总血量按顺序灌满每一条
            int remaining = Mathf.Max(0, value);

            for (int i = 0; i < segmentCurrentHPs.Length; i++)
            {
                int max = Mathf.Max(1, segmentMaxHPs[i]);
                int newCur = Mathf.Clamp(remaining, 0, max);
                segmentCurrentHPs[i] = newCur;
                remaining -= newCur;
            }

            // 计算修改后总血量
            int newTotal = 0;
            for (int i = 0; i < segmentCurrentHPs.Length; i++)
                newTotal += Mathf.Max(0, segmentCurrentHPs[i]);

            int delta = newTotal - oldTotal;
            if (delta < 0)
                ShowDamageNumber(-delta);
            else if (delta > 0)
                ShowHealNumber(delta);

            Debug.Log($"[Token] 设置敌人总当前 HP 为 {value}（多条血）: {GetSegmentsDebug()}");
            NotifyAll();

            // 全部为 0 视为死亡
            bool allZero = true;
            for (int i = 0; i < segmentCurrentHPs.Length; i++)
            {
                if (segmentCurrentHPs[i] > 0)
                {
                    allZero = false;
                    break;
                }
            }
            if (allZero)
            {
                Debug.Log("敌人死亡（Token 修改多条血）");
                OnDead?.Invoke();
            }
        }
        else
        {
            // 单条血逻辑
            maxHP = Mathf.Max(1, maxHP);
            int old = currentHP;
            currentHP = Mathf.Clamp(value, 0, maxHP);
            int delta = currentHP - old;

            if (delta < 0)
                ShowDamageNumber(-delta);
            else if (delta > 0)
                ShowHealNumber(delta);

            Debug.Log($"[Token] 设置敌人当前 HP 为 {currentHP}/{maxHP}");
            NotifyAll();

            if (currentHP <= 0)
            {
                Debug.Log("敌人死亡（Token 修改单条血）");
                OnDead?.Invoke();
            }
        }
    }

    /// <summary>
    /// Token：设置敌人的“总最大 HP”
    /// （单条血直接改 maxHP；多条血时简单放大/缩小每一条的最大值）
    /// </summary>
    public void SetMaxHPByToken(int value, bool clampCurrent = true)
    {
        if (useSegments && segmentCurrentHPs != null && segmentMaxHPs != null)
        {
            int oldTotalMax = 0;
            for (int i = 0; i < segmentMaxHPs.Length; i++)
                oldTotalMax += Mathf.Max(1, segmentMaxHPs[i]);

            int newTotalMax = Mathf.Max(1, value);

            // 简单做法：按比例缩放每一条血的最大值
            for (int i = 0; i < segmentMaxHPs.Length; i++)
            {
                float ratio = oldTotalMax > 0 ? (float)segmentMaxHPs[i] / oldTotalMax : 1f / segmentMaxHPs.Length;
                int newMax = Mathf.Max(1, Mathf.RoundToInt(newTotalMax * ratio));

                int oldCur = segmentCurrentHPs[i];
                segmentMaxHPs[i] = newMax;

                if (clampCurrent)
                {
                    segmentCurrentHPs[i] = Mathf.Clamp(segmentCurrentHPs[i], 0, newMax);
                    int delta = segmentCurrentHPs[i] - oldCur;

                    if (delta < 0)
                        ShowDamageNumber(-delta);
                    else if (delta > 0)
                        ShowHealNumber(delta);
                }
            }

            Debug.Log($"[Token] 设置敌人总最大 HP 为 {newTotalMax}（多条血）：{GetSegmentsDebug()}");
            NotifyAll();
        }
        else
        {
            int old = currentHP;
            maxHP = Mathf.Max(1, value);
            if (clampCurrent)
            {
                currentHP = Mathf.Clamp(currentHP, 0, maxHP);
                int delta = currentHP - old;

                if (delta < 0)
                    ShowDamageNumber(-delta);
                else if (delta > 0)
                    ShowHealNumber(delta);
            }

            Debug.Log($"[Token] 设置敌人最大 HP 为 {maxHP}，当前 {currentHP}");
            NotifyAll();
        }
    }

    // ----------- 数字显示 -----------

    private void ShowDamageNumber(int amount)
    {
        if (amount <= 0)
        {
            Debug.Log("[DamageNumber] amount <= 0，不生成伤害数字");
            return;
        }

        if (damageNumberPrefab == null)
        {
            Debug.LogWarning("[DamageNumber] damageNumberPrefab 未绑定");
            return;
        }

        if (damageNumberParent == null)
        {
            Debug.LogWarning("[DamageNumber] damageNumberParent 未绑定");
            return;
        }

        Vector2 anchoredPos = damageNumberOffset;   // ← 使用伤害 offset
        damageNumberPrefab.SpawnGUI(damageNumberParent, anchoredPos, amount);

        Debug.Log($"[DamageNumber] SpawnGUI（伤害）：parent={damageNumberParent.name}, anchoredPos={anchoredPos}, amount={amount}");
    }

    private void ShowHealNumber(int amount)
    {
        if (amount <= 0)
        {
            Debug.Log("[HealNumber] amount <= 0，不生成治疗数字");
            return;
        }

        if (healNumberPrefab == null)
        {
            Debug.LogWarning("[HealNumber] healNumberPrefab 未绑定");
            return;
        }

        if (damageNumberParent == null)
        {
            Debug.LogWarning("[HealNumber] damageNumberParent 未绑定");
            return;
        }

        Vector2 anchoredPos = healNumberOffset;     // ← 使用治疗 offset
        healNumberPrefab.SpawnGUI(damageNumberParent, anchoredPos, amount);

        Debug.Log($"[HealNumber] SpawnGUI（治疗）：parent={damageNumberParent.name}, anchoredPos={anchoredPos}, amount={amount}");
    }

    // ---------------- 通知 UI & debug ----------------

    private void NotifyAll()
    {
        if (useSegments && segmentCurrentHPs != null && segmentMaxHPs != null)
        {
            OnSegmentsChanged?.Invoke(segmentCurrentHPs, segmentMaxHPs);

            int totalCur = 0;
            int totalMax = 0;
            for (int i = 0; i < segmentCurrentHPs.Length; i++)
            {
                totalCur += Mathf.Clamp(segmentCurrentHPs[i], 0, segmentMaxHPs[i]);
                totalMax += Mathf.Max(0, segmentMaxHPs[i]);
            }

            OnHPChanged?.Invoke(totalCur, totalMax);
        }
        else
        {
            OnHPChanged?.Invoke(currentHP, maxHP);
        }
    }

    private string GetSegmentsDebug()
    {
        if (!useSegments || segmentCurrentHPs == null || segmentMaxHPs == null)
            return $"单条 {currentHP}/{maxHP}";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("[ ");
        for (int i = 0; i < segmentCurrentHPs.Length; i++)
        {
            sb.Append($"{segmentCurrentHPs[i]}/{segmentMaxHPs[i]} ");
        }
        sb.Append("]");
        return sb.ToString();
    }
}
