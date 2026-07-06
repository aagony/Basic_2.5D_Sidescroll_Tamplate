# MonsterStatus 제거 리팩토링 마이그레이션 가이드

## 목표

- MonsterStatus.cs 의존성을 제거합니다.
- TakeDamage는 float 데미지만 처리합니다.
- 피격 위치와 공격자와 방향 같은 부가 정보는 DamageHitEvent로 전달합니다.
- Player Bullet과 Monster Projectile은 당장 하나로 합치지 않고 IDamageable 계약만 공유합니다.

## 새 공통 계약

- IDamageable
- IDeadState
- ICombatStatsProvider
- DamageHitEvent
- HealthChangedEvent
- ProjectileBlockedEvent

## 몬스터 루트 프리팹에 붙일 컴포넌트

- MonsterHealth
- MonsterStats
- MonsterFacing
- MonsterTargetSensor
- MonsterDeathHandler

지상 몬스터에는 추가로 붙입니다.

- CharacterController
- MonsterGroundMotor

공중 몬스터에는 기존 MonsterFlyingMotor를 유지합니다.

## 옮길 스크립트

다음 스크립트는 Assets/_Deprecated 폴더로 이동합니다.

- MonsterStatus.cs
- DamageInfo.cs
- MonsterProjectile.cs
- BulletProjectile.cs
- 더 이상 참조하지 않는 레거시 투사체 스크립트

## 수정할 코드 검색어

프로젝트 전체에서 아래 키워드가 남아있지 않게 정리합니다.

- MonsterStatus
- SelfStatus
- DamageInfo
- Status.IsDead
- GetCurrentHP

## Player Bullet 수정 핵심

기존 TryGetComponent<IDamageable> 대신 GetComponentInParent<IDamageable>를 사용합니다.
자식 HitCollider에 맞아도 루트의 MonsterHealth를 찾기 위함입니다.

## DefenseStageController 수정 핵심

MonsterStatus 대신 IDeadState를 찾습니다.
MonsterDeadEvent는 계속 사용합니다.
