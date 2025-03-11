using Assets.CoreFighter.Scripts.Configs;
using PugProperties;
using PugTilemap;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

namespace Assets.CoreFighter.Scripts.Systems.Misc
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateBefore(typeof(ExplosionDamageSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    public partial class BlockExplosionSystem : PugSimulationSystemBase
    {
        private NativeList<int2> tileHitPositions;
        private AttackSystem.Helper attackHelper;
        private TileAccessor tileAccessor;
        private BufferLookup<LevelEntitiesBuffer> levelEntitiesBufferLookup;
        private ComponentLookup<LevelCD> levelLookup;
        private ComponentLookup<OwnerCD> ownerLookup;
        private ComponentLookup<PlayerGhost> playerGhostLookup;
        private ComponentLookup<IndestructibleCD> indesctructibleLookup;
        protected override void OnCreate()
        {
            tileHitPositions = new NativeList<int2>(16, Allocator.Persistent);
            RequireForUpdate<ServerSeedCD>();
            RequireForUpdate<PhysicsWorldSingleton>();
            RequireForUpdate<ClientServerTickRate>();
            RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            RequireForUpdate<PugDatabase.DatabaseBankCD>();
            RequireForUpdate<ClientServerTickRate>();
            RequireForUpdate<TileDamageBuffer>();
            RequireForUpdate<TileUpdateBuffer>();
            RequireForUpdate<EffectEventBuffer>();
            RequireForUpdate<WorldInfoCD>();
            RequireForUpdate<ExplosionCD>();
            levelEntitiesBufferLookup = SystemAPI.GetBufferLookup<LevelEntitiesBuffer>();
            levelLookup = SystemAPI.GetComponentLookup<LevelCD>();
            ownerLookup = SystemAPI.GetComponentLookup<OwnerCD>();
            playerGhostLookup = SystemAPI.GetComponentLookup<PlayerGhost>();
            indesctructibleLookup = SystemAPI.GetComponentLookup<IndestructibleCD>();
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            attackHelper = GetAttackHelper();
            tileAccessor = CreateTileAccessor();
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            if (!FighterConfig.IsEnable(FighterCategory.Misc, FC_Misc.ImmuneExplosion))
                return;
            var attackHelper = this.attackHelper;
            var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
            var tickRate = (uint)SystemAPI.GetSingleton<ClientServerTickRate>().SimulationTickRate;
            var ecb = CreateCommandBuffer();
            var tileDamageBuffer = SystemAPI.GetSingletonBuffer<TileDamageBuffer>();
            var tileUpdateBuffer = SystemAPI.GetSingletonBuffer<TileUpdateBuffer>();
            var tileHitPositions = this.tileHitPositions;
            var eventBufferEntity = SystemAPI.GetSingletonEntity<EffectEventBuffer>();
            var tileAccessor = this.tileAccessor;
            var ownerLookup = this.ownerLookup;
            var levelLookup = this.levelLookup;
            var playerGhostLookup = this.playerGhostLookup;
            var levelEntitiesBufferLookup = this.levelEntitiesBufferLookup;
            var indesctructibleLookup = this.indesctructibleLookup;
            var database = this.database;
            bool isServer = this.isServer;
            Entities.ForEach((Entity entity, in BehaviourTagsCD attackTags,
                in ObjectDataCD objectData, in GhostInstance ghostInstance) =>
            {
                LocalTransform localTransform = attackHelper.localTransformLookup[entity];
                ref ExplosionCD valueRW = ref attackHelper.explosionLookup.GetRefRW(entity).ValueRW;
                if (valueRW.hasDealtDamage)
                {
                    return;
                }
                if (!valueRW.delayTimer.isRunning)
                {
                    valueRW.delayTimer.Start(currentTick, 0.1f, tickRate);
                }
                if (!valueRW.delayTimer.IsTimerElapsed(currentTick))
                {
                    return;
                }
                if (valueRW.spawnNapalm)
                {
                    SpawnNapalm(localTransform.Position, valueRW.level, valueRW.napalmIncreasedBurningDamagePercentage, ecb, attackHelper.propertiesLookup, attackHelper.attackContinuouslyLookup, levelEntitiesBufferLookup, levelLookup, attackHelper.conditionsBufferLookup, attackHelper.databaseBank);
                }
                valueRW.hasDealtDamage = true;
                tileHitPositions.Clear();
                int damage = valueRW.damage;
                int tileDamage = valueRW.tileDamage;
                bool flag = false;
                Entity causedByEntity = entity;
                if (ownerLookup.TryGetComponent(entity, out OwnerCD ownerCD))
                {
                    causedByEntity = ownerCD.owner;
                    flag = playerGhostLookup.HasComponent(ownerCD.owner);
                }
                if (!flag && !isServer)
                {
                    return;
                }
                AttackSystem.Helper.Parameters parameters = new()
                {
                    effectEventBufferSingleton = eventBufferEntity,
                    attacker = entity,
                    radius = valueRW.radius,
                    damage = damage,
                    playerDamage = 0,
                    pushback = 2f,
                    bypassMaxDamagePerHit = true,
                    canHitLowTriggers = true,
                    behaviourTags = attackTags,
                    canAttackOwner = true,
                    isExplosive = true,
                    isPredicted = flag,
                    skipHitsOnEntity = ((valueRW.triggerEntityToIgnoreExplosionDamage != Entity.Null) ? valueRW.triggerEntityToIgnoreExplosionDamage : valueRW.nonSyncedTriggerEntityToIgnoreExplosionDamage)
                };
                attackHelper.Attack(ecb, ref tileHitPositions, parameters);
                bool flag2 = false;
                NativeList<DistanceHit> nativeList = new(Allocator.Temp);
                if (attackHelper.physicsWorld.CollisionWorld.OverlapSphere(localTransform.Position,
                    valueRW.radius, ref nativeList, new CollisionFilter
                {
                    BelongsTo = 4294967295U,
                    CollidesWith = 1024U
                }, QueryInteraction.Default))
                {
                    for (int i = 0; i < nativeList.Length; i++)
                    {
                        if (indesctructibleLookup.IsComponentEnabled(nativeList[i].Entity))
                        {
                            flag2 = true;
                            break;
                        }
                    }
                }
                nativeList.Dispose();
                float2 @float = new(localTransform.Position.x, localTransform.Position.z);
                int2 rhs = new((int)math.round(@float.x), (int)math.round(@float.y));
                for (int j = -4; j <= 4; j++)
                {
                    for (int k = -4; k <= 4; k++)
                    {
                        int2 @int = new int2(j, k) + rhs;
                        if (math.distance(@int, @float) <= valueRW.radius)
                        {
                            tileDamageBuffer.Add(new TileDamageBuffer
                            {
                                damage = tileDamage,
                                position = @int,
                                canHitLowColliders = true,
                                bypassMaxDamagePerHit = true,
                                damagedByExplosion = true,
                                causedByEntity = causedByEntity
                            });
                            if (!flag2)
                            {
                                TileCD top = tileAccessor.GetTop(@int);
                                if (top.tileType == TileType.ground && !tileHitPositions.Contains(@int)
                                && PugDatabase.TileExists(top.tileset, TileType.dugUpGround, database))
                                {
                                    tileUpdateBuffer.Add(new TileUpdateBuffer
                                    {
                                        command = TileUpdateBuffer.Command.Add,
                                        position = @int,
                                        tile = new TileCD
                                        {
                                            tileset = top.tileset,
                                            tileType = TileType.dugUpGround
                                        }
                                    });
                                }
                            }
                        }
                    }
                }
            })
                .WithName("BlockHurtByExplosion")
                .WithAll<ExplosionCD>()
                .WithAll<Simulate>()
                .WithAll<LocalTransform>()
                .WithNone<EntityDestroyedCD>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
        private static void SpawnNapalm(float3 position, int level, int increasedBurningDamagePercentage, EntityCommandBuffer ecb, ComponentLookup<ObjectPropertiesCD> objectPropertiesCDLookup, ComponentLookup<AttackContinuouslyCD> attackContinuouslyLookup, BufferLookup<LevelEntitiesBuffer> levelEntitiesBufferLookup, ComponentLookup<LevelCD> levelLookup, BufferLookup<ConditionsBuffer> conditionsBufferLookup, PugDatabase.DatabaseBankCD databaseBankCD)
        {
            // 创建 Napalm 实体
            Entity entity = EntityUtility.CreateEntity(ecb, position, ObjectID.Napalm, 1, databaseBankCD.databaseBankBlob, out Entity entity2, 0);

            // 设置实体的等级
            ecb.SetComponent(entity, new LevelCD
            {
                level = level
            });

            // 创建对象数据组件
            ObjectDataCD objectData = new()
            {
                objectID = ObjectID.Napalm,
                variation = level
            };

            // 获取与实体关联的等级实体
            Entity levelEntity = EntityUtility.GetLevelEntity(entity2, objectData, levelEntitiesBufferLookup, levelLookup);

            // 如果等级实体存在，处理条件缓冲区
            if (levelEntity != Entity.Null)
            {
                ecb.SetBuffer<ConditionsBuffer>(entity);
                conditionsBufferLookup.TryGetBuffer(levelEntity, out DynamicBuffer<ConditionsBuffer> dynamicBuffer);

                // 遍历条件缓冲区并调整燃烧伤害
                for (int i = 0; i < dynamicBuffer.Length; i++)
                {
                    ConditionsBuffer conditionsBuffer = dynamicBuffer[i];
                    int num = (int)math.round(conditionsBuffer.condition.conditionData.value * increasedBurningDamagePercentage / 100f);
                    conditionsBuffer.condition.conditionData.value += num;
                    ecb.AppendToBuffer(entity, conditionsBuffer);
                }
            }

            // 获取 AttackContinuouslyCD 组件的只读引用
            if (attackContinuouslyLookup.TryGetComponent(entity2, out AttackContinuouslyCD component))
            {
                // 获取对象属性组件
                objectPropertiesCDLookup.TryGetComponent(entity2, out ObjectPropertiesCD objectPropertiesCD);

                // 如果存在伤害乘数，则调整伤害值
                if (objectPropertiesCD.TryGet(-555946377, out float damageMultiplier))
                {
                    component.damage = AttackContinuouslyAuthoring.LevelToDamage(level, damageMultiplier);
                }

                // 设置实体的 AttackContinuouslyCD 组件
                ecb.SetComponent(entity, component);
            }
        }
    }
}
