using System.ComponentModel.DataAnnotations;

namespace EzItam.Contracts;

public enum AssetStatus
{
    [Display(Name = "재고")] InStock = 0,
    [Display(Name = "사용중")] Assigned = 1,
    [Display(Name = "수리중")] InRepair = 2,
    [Display(Name = "유휴")] Idle = 3,
    [Display(Name = "불용")] Retired = 4,
    [Display(Name = "폐기")] Disposed = 5,
    [Display(Name = "분실")] Lost = 6
}

public enum MaintenanceType
{
    [Display(Name = "수리")] Repair = 0,
    [Display(Name = "업그레이드")] Upgrade = 1,
    [Display(Name = "점검")] Inspection = 2,
    [Display(Name = "교체")] Replacement = 3
}

public enum LicenseType
{
    [Display(Name = "사용자(Per Seat)")] PerSeat = 0,
    [Display(Name = "장비(Per Device)")] PerDevice = 1,
    [Display(Name = "구독")] Subscription = 2,
    [Display(Name = "사이트")] Site = 3,
    [Display(Name = "오픈소스/무료")] Free = 4
}

public enum AuditAction
{
    Created = 0,
    Updated = 1,
    Deleted = 2
}

public enum AgentStatus
{
    [Display(Name = "승인 대기")] Pending = 0,
    [Display(Name = "승인됨")] Approved = 1,
    [Display(Name = "사용 중지")] Disabled = 2
}

/// <summary>업데이트 배포 링. Pilot 은 먼저 받고, Stable 은 검증 후 받습니다.</summary>
public enum AgentRing
{
    Stable = 0,
    Pilot = 1
}

public enum InventoryStatus
{
    Open = 1,
    InProgress = 2,
    Closed = 3
}
