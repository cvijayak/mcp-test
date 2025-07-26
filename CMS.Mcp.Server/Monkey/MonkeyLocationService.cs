namespace CMS.Mcp.Server.Monkey
{
    using System;
    using System.Collections.Generic;
    using Contracts.Monkey;

    public class MonkeyLocationService {
        private readonly Random _random = new();
        private readonly Dictionary<string, MonkeyTypeConfig> _monkeyConfigs;

        public MonkeyLocationService() {
            _monkeyConfigs = InitializeMonkeyConfigs();
        }

        public MonkeyJourney GenerateMonkeyJourney(Monkey monkey) {
            var baseLocation = new GeoLocation(monkey.Latitude, monkey.Longitude);
            var config = GetMonkeyConfig(monkey.Name ?? "Unknown");

            var journey = new MonkeyJourney {
                MonkeyName = monkey.Name ?? "Unknown",
                StartLocation = baseLocation,
                StartTime = DateTime.UtcNow.AddHours(-_random.Next(1, 12)),
                PathPoints = GeneratePathPoints(baseLocation, config),
                Activities = [],
                HealthStats = GenerateHealthStats(config)
            };

            journey.Activities = GenerateActivities(journey.PathPoints, config);
            journey.EndTime = journey.StartTime.AddMinutes(journey.PathPoints.Count * _random.Next(5, 15));

            return journey;
        }

        private List<PathPoint> GeneratePathPoints(GeoLocation startLocation, MonkeyTypeConfig config) {
            var pathPoints = new List<PathPoint> { new() { Location = startLocation, Timestamp = DateTime.UtcNow.AddHours(-_random.Next(1, 12)) } };

            var currentLat = startLocation.Latitude;
            var currentLon = startLocation.Longitude;
            var numPoints = _random.Next(10, 20);

            for (int i = 1; i <= numPoints; i++) {
                var moveDistance = config.MaxMovementRadius * _random.NextDouble();
                var moveAngle = _random.NextDouble() * 2 * Math.PI;

                if (config.PreferredTerrain == "Forest") {
                    moveDistance *= 0.7;
                } else if (config.PreferredTerrain == "Mountain") {
                    moveDistance *= 1.2;
                }

                currentLat += moveDistance * Math.Cos(moveAngle) * 0.001;
                currentLon += moveDistance * Math.Sin(moveAngle) * 0.001;

                pathPoints.Add(new PathPoint {
                    Location = new GeoLocation(currentLat, currentLon),
                    Timestamp = pathPoints[0].Timestamp.AddMinutes(i * _random.Next(10, 30))
                });
            }

            return pathPoints;
        }

        private List<MonkeyActivity> GenerateActivities(List<PathPoint> pathPoints, MonkeyTypeConfig config) {
            var activities = new List<MonkeyActivity>();
            var activityChance = 0.3;

            for (int i = 1; i < pathPoints.Count; i++) {
                if (_random.NextDouble() < activityChance) {
                    var activity = SelectRandomActivity(config);
                    activities.Add(new MonkeyActivity {
                        Type = activity.Type,
                        Description = activity.Description,
                        Location = pathPoints[i].Location,
                        Timestamp = pathPoints[i].Timestamp,
                        Duration = TimeSpan.FromMinutes(_random.Next(activity.MinDuration, activity.MaxDuration + 1)),
                        EnergyChange = activity.EnergyChange + _random.Next(-5, 6)
                    });
                }
            }

            return activities;
        }

        private MonkeyHealthStats GenerateHealthStats(MonkeyTypeConfig config) {
            return new MonkeyHealthStats {
                Energy = _random.Next(config.BaseEnergy - 20, config.BaseEnergy + 21),
                Happiness = _random.Next(60, 101),
                Hunger = _random.Next(20, 80),
                Social = _random.Next(config.BaseSocial - 15, config.BaseSocial + 16),
                Stress = _random.Next(10, 50),
                Health = _random.Next(80, 101)
            };
        }

        private ActivityTemplate SelectRandomActivity(MonkeyTypeConfig config)
        {
            if (_random.NextDouble() < 0.4 && config.PreferredActivities.Count > 0) {
                return config.PreferredActivities[_random.Next(config.PreferredActivities.Count)];
            }

            var commonActivities = GetCommonActivities();
            return commonActivities[_random.Next(commonActivities.Count)];
        }

        private List<ActivityTemplate> GetCommonActivities() {
            return
            [
                new()
                {
                    Type = "Foraging", Description = "Found some delicious fruits and ate them", MinDuration = 5,
                    MaxDuration = 20, EnergyChange = 10
                },
                new()
                {
                    Type = "Resting", Description = "Took a well-deserved nap under a tree", MinDuration = 15,
                    MaxDuration = 45, EnergyChange = 15
                },
                new()
                {
                    Type = "Exploring", Description = "Investigated an interesting rock formation", MinDuration = 5,
                    MaxDuration = 15, EnergyChange = -5
                },
                new()
                {
                    Type = "Drinking", Description = "Found a water source and had a refreshing drink", MinDuration = 2,
                    MaxDuration = 8, EnergyChange = 5
                },
                new()
                {
                    Type = "Climbing", Description = "Climbed a tall tree to get a better view", MinDuration = 5,
                    MaxDuration = 15, EnergyChange = -10
                },
                new()
                {
                    Type = "Sunbathing", Description = "Relaxed in a warm sunny spot", MinDuration = 10,
                    MaxDuration = 30, EnergyChange = 8
                },
                new()
                {
                    Type = "Playing", Description = "Engaged in playful antics with nearby objects", MinDuration = 5,
                    MaxDuration = 20, EnergyChange = -3
                },
                new()
                {
                    Type = "Grooming", Description = "Spent time cleaning and grooming themselves", MinDuration = 8,
                    MaxDuration = 25, EnergyChange = 3
                },
                new()
                {
                    Type = "Watching", Description = "Sat quietly observing the surrounding environment",
                    MinDuration = 3, MaxDuration = 15, EnergyChange = 2
                },
                new()
                {
                    Type = "Stretching", Description = "Did some stretching exercises after sitting for a while",
                    MinDuration = 2, MaxDuration = 8, EnergyChange = 5
                },
                new()
                {
                    Type = "Snacking", Description = "Found and munched on some seeds or small insects",
                    MinDuration = 3, MaxDuration = 12, EnergyChange = 7
                },
                new()
                {
                    Type = "Shelter Seeking", Description = "Found a cozy spot for protection from the elements",
                    MinDuration = 5, MaxDuration = 20, EnergyChange = 4
                },
                new()
                {
                    Type = "Scent Investigation", Description = "Investigated interesting scents in the area",
                    MinDuration = 2, MaxDuration = 10, EnergyChange = -2
                },
                new()
                {
                    Type = "Branch Swinging", Description = "Swung energetically from branch to branch",
                    MinDuration = 3, MaxDuration = 12, EnergyChange = -8
                },
                new()
                {
                    Type = "Ground Exploration", Description = "Searched the ground for interesting items or food",
                    MinDuration = 8, MaxDuration = 25, EnergyChange = -5
                }
            ];
        }

        private MonkeyTypeConfig GetMonkeyConfig(string monkeyName) {
            foreach (var config in _monkeyConfigs) {
                if (monkeyName.Contains(config.Key, StringComparison.OrdinalIgnoreCase)) {
                    return config.Value;
                }
            }

            return _monkeyConfigs["Default"];
        }

        private Dictionary<string, MonkeyTypeConfig> InitializeMonkeyConfigs() {
            return new Dictionary<string, MonkeyTypeConfig>
            {
                {
                    "Baboon",
                    new MonkeyTypeConfig
                    {
                        MaxMovementRadius = 2.0,
                        PreferredTerrain = "Savanna",
                        BaseEnergy = 80,
                        BaseSocial = 90,
                        PreferredActivities =
                        [
                            new()
                            {
                                Type = "Grooming", Description = "Spent time grooming another baboon's fur",
                                MinDuration = 10, MaxDuration = 30, EnergyChange = 5
                            },
                            new()
                            {
                                Type = "Socializing", Description = "Had an animated conversation with the troop",
                                MinDuration = 15, MaxDuration = 25, EnergyChange = 0
                            },
                            new()
                            {
                                Type = "Foraging", Description = "Dug up some tasty roots and tubers", MinDuration = 10,
                                MaxDuration = 25, EnergyChange = 15
                            }
                        ]
                    }
                },
                {
                    "Capuchin",
                    new MonkeyTypeConfig
                    {
                        MaxMovementRadius = 1.8,
                        PreferredTerrain = "Rainforest",
                        BaseEnergy = 85,
                        BaseSocial = 80,
                        PreferredActivities =
                        [
                            new()
                            {
                                Type = "Tool Use", Description = "Used a stick to extract insects from tree bark",
                                MinDuration = 8, MaxDuration = 20, EnergyChange = 10
                            },
                            new()
                            {
                                Type = "Fruit Foraging", Description = "Found and cracked open some nutritious nuts",
                                MinDuration = 10, MaxDuration = 25, EnergyChange = 15
                            },
                            new()
                            {
                                Type = "Social Learning",
                                Description = "Watched and learned new techniques from other capuchins",
                                MinDuration = 5, MaxDuration = 15, EnergyChange = 0
                            }
                        ]
                    }
                },
                {
                    "Blue",
                    new MonkeyTypeConfig
                    {
                        MaxMovementRadius = 1.6,
                        PreferredTerrain = "Forest",
                        BaseEnergy = 75,
                        BaseSocial = 85,
                        PreferredActivities =
                        [
                            new()
                            {
                                Type = "Canopy Foraging", Description = "Searched for fruits high in the forest canopy",
                                MinDuration = 12, MaxDuration = 30, EnergyChange = 12
                            },
                            new()
                            {
                                Type = "Territory Patrol", Description = "Patrolled the group's territory boundaries",
                                MinDuration = 20, MaxDuration = 40, EnergyChange = -8
                            },
                            new()
                            {
                                Type = "Group Bonding", Description = "Engaged in social bonding with troop members",
                                MinDuration = 10, MaxDuration = 25, EnergyChange = 5
                            }
                        ]
                    }
                },
                {
                    "Squirrel",
                    new MonkeyTypeConfig
                    {
                        MaxMovementRadius = 1.4,
                        PreferredTerrain = "Rainforest",
                        BaseEnergy = 90,
                        BaseSocial = 95,
                        PreferredActivities =
                        [
                            new()
                            {
                                Type = "Acrobatic Leaping", Description = "Performed impressive leaps between branches",
                                MinDuration = 2, MaxDuration = 8, EnergyChange = -5
                            },
                            new()
                            {
                                Type = "Insect Hunting", Description = "Caught and ate some protein-rich insects",
                                MinDuration = 5, MaxDuration = 15, EnergyChange = 8
                            },
                            new()
                            {
                                Type = "Playful Wrestling",
                                Description = "Engaged in playful wrestling with other squirrel monkeys",
                                MinDuration = 8, MaxDuration = 20, EnergyChange = 0
                            }
                        ]
                    }
                },
                {
                    "Golden Lion Tamarin",
                    new MonkeyTypeConfig
                    {
                        MaxMovementRadius = 1.3,
                        PreferredTerrain = "Atlantic Forest",
                        BaseEnergy = 80,
                        BaseSocial = 90,
                        PreferredActivities =
                        [
                            new()
                            {
                                Type = "Mane Grooming", Description = "Carefully groomed their magnificent golden mane",
                                MinDuration = 10, MaxDuration = 25, EnergyChange = 5
                            },
                            new()
                            {
                                Type = "Tree Hole Foraging",
                                Description = "Searched tree holes for hidden insects and small animals",
                                MinDuration = 8, MaxDuration = 20, EnergyChange = 12
                            },
                            new()
                            {
                                Type = "Family Bonding", Description = "Spent quality time with family members",
                                MinDuration = 15, MaxDuration = 35, EnergyChange = 8
                            }
                        ]
                    }
                },
                {
                    "Howler",
                    new MonkeyTypeConfig
                    {
                        MaxMovementRadius = 1.5,
                        PreferredTerrain = "Forest",
                        BaseEnergy = 70,
                        BaseSocial = 75,
                        PreferredActivities =
                        [
                            new()
                            {
                                Type = "Howling",
                                Description = "Let out a mighty roar to communicate with distant howlers",
                                MinDuration = 2, MaxDuration = 5, EnergyChange = -5
                            },
                            new()
                            {
                                Type = "Leaf Eating", Description = "Munched on some fresh, tender leaves",
                                MinDuration = 15, MaxDuration = 35, EnergyChange = 12
                            },
                            new()
                            {
                                Type = "Territory Marking",
                                Description = "Marked territory with scent to ward off intruders", MinDuration = 3,
                                MaxDuration = 8, EnergyChange = -3
                            }
                        ]
                    }
                },
                {
                    "Japanese Macaque",
                    new MonkeyTypeConfig
                    {
                        MaxMovementRadius = 1.8,
                        PreferredTerrain = "Mountain",
                        BaseEnergy = 85,
                        BaseSocial = 85,
                        PreferredActivities =
                        [
                            new()
                            {
                                Type = "Hot Spring Bath", Description = "Relaxed in a natural hot spring",
                                MinDuration = 20, MaxDuration = 45, EnergyChange = 20
                            },
                            new()
                            {
                                Type = "Snow Play", Description = "Made and threw snowballs for fun", MinDuration = 10,
                                MaxDuration = 20, EnergyChange = -8
                            },
                            new()
                            {
                                Type = "Potato Washing", Description = "Carefully washed sweet potatoes in a stream",
                                MinDuration = 5, MaxDuration = 12, EnergyChange = 8
                            }
                        ]
                    }
                },
                {
                    "Mandrill",
                    new MonkeyTypeConfig
                    {
                        MaxMovementRadius = 2.2,
                        PreferredTerrain = "Rainforest",
                        BaseEnergy = 85,
                        BaseSocial = 95,
                        PreferredActivities =
                        [
                            new()
                            {
                                Type = "Colorful Display",
                                Description = "Showed off their vibrant facial colors to assert dominance",
                                MinDuration = 3, MaxDuration = 10, EnergyChange = -3
                            },
                            new()
                            {
                                Type = "Ground Foraging",
                                Description = "Searched the forest floor for fruits and roots", MinDuration = 15,
                                MaxDuration = 30, EnergyChange = 15
                            },
                            new()
                            {
                                Type = "Troop Leadership", Description = "Led the troop to new foraging areas",
                                MinDuration = 20, MaxDuration = 45, EnergyChange = -10
                            }
                        ]
                    }
                },
                {
                    "Proboscis",
                    new MonkeyTypeConfig
                    {
                        MaxMovementRadius = 1.2,
                        PreferredTerrain = "Mangrove",
                        BaseEnergy = 75,
                        BaseSocial = 70,
                        PreferredActivities =
                        [
                            new()
                            {
                                Type = "Swimming", Description = "Took a refreshing swim across a river",
                                MinDuration = 8, MaxDuration = 20, EnergyChange = -5
                            },
                            new()
                            {
                                Type = "Nose Flexing",
                                Description = "Showed off impressive nose size to potential mates", MinDuration = 3,
                                MaxDuration = 8, EnergyChange = -2
                            },
                            new()
                            {
                                Type = "Mangrove Foraging", Description = "Found delicious mangrove fruits and seeds",
                                MinDuration = 12, MaxDuration = 25, EnergyChange = 14
                            }
                        ]
                    }
                },
                {
                    "Red-shanked douc",
                    new MonkeyTypeConfig
                    {
                        MaxMovementRadius = 1.7,
                        PreferredTerrain = "Forest",
                        BaseEnergy = 78,
                        BaseSocial = 82,
                        PreferredActivities =
                        [
                            new()
                            {
                                Type = "Colorful Posing",
                                Description = "Displayed their beautiful colorful fur in the sunlight", MinDuration = 5,
                                MaxDuration = 15, EnergyChange = 0
                            },
                            new()
                            {
                                Type = "Leaf Selection",
                                Description = "Carefully selected the most nutritious young leaves", MinDuration = 10,
                                MaxDuration = 25, EnergyChange = 10
                            },
                            new()
                            {
                                Type = "Arboreal Movement", Description = "Gracefully moved through the forest canopy",
                                MinDuration = 8, MaxDuration = 20, EnergyChange = -5
                            }
                        ]
                    }
                },
                {
                    "Sebastian",
                    new MonkeyTypeConfig
                    {
                        MaxMovementRadius = 0.5,
                        PreferredTerrain = "Urban",
                        BaseEnergy = 95,
                        BaseSocial = 100,
                        PreferredActivities =
                        [
                            new()
                            {
                                Type = "Tech Shopping",
                                Description = "Browsed the latest Android devices at the local electronics store",
                                MinDuration = 30, MaxDuration = 60, EnergyChange = 10
                            },
                            new()
                            {
                                Type = "Coffee Break", Description = "Enjoyed a latte at a trendy Seattle coffee shop",
                                MinDuration = 15, MaxDuration = 30, EnergyChange = 15
                            },
                            new()
                            {
                                Type = "Social Media", Description = "Posted updates and photos on @MotzMonkeys",
                                MinDuration = 10, MaxDuration = 20, EnergyChange = 5
                            }
                        ]
                    }
                },
                {
                    "Henry",
                    new MonkeyTypeConfig
                    {
                        MaxMovementRadius = 0.8,
                        PreferredTerrain = "Desert Urban",
                        BaseEnergy = 88,
                        BaseSocial = 92,
                        PreferredActivities =
                        [
                            new()
                            {
                                Type = "iOS Testing", Description = "Tested the latest iPhone features and apps",
                                MinDuration = 25, MaxDuration = 45, EnergyChange = 8
                            },
                            new()
                            {
                                Type = "Desert Exploration",
                                Description = "Explored the beautiful Arizona desert landscapes", MinDuration = 40,
                                MaxDuration = 80, EnergyChange = -10
                            },
                            new()
                            {
                                Type = "Travel Planning", Description = "Planned the next adventure with Heather",
                                MinDuration = 20, MaxDuration = 40, EnergyChange = 5
                            }
                        ]
                    }
                },
                {
                    "Mooch",
                    new MonkeyTypeConfig
                    {
                        MaxMovementRadius = 0.6,
                        PreferredTerrain = "Urban",
                        BaseEnergy = 90,
                        BaseSocial = 98,
                        PreferredActivities =
                        [
                            new()
                            {
                                Type = "iPhone Photography",
                                Description = "Captured stunning photos with the latest iPhone camera",
                                MinDuration = 15, MaxDuration = 35, EnergyChange = 5
                            },
                            new()
                            {
                                Type = "Waterfront Stroll",
                                Description = "Enjoyed a peaceful walk along the Seattle waterfront", MinDuration = 30,
                                MaxDuration = 60, EnergyChange = 10
                            },
                            new()
                            {
                                Type = "Adventure Blogging",
                                Description = "Documented travel adventures for @MotzMonkeys followers",
                                MinDuration = 20, MaxDuration = 40, EnergyChange = 0
                            }
                        ]
                    }
                },
                {
                    "Default",
                    new MonkeyTypeConfig
                    {
                        MaxMovementRadius = 1.5,
                        PreferredTerrain = "Forest",
                        BaseEnergy = 75,
                        BaseSocial = 75,
                        PreferredActivities =
                        [
                            new()
                            {
                                Type = "Banana Finding",
                                Description = "Discovered a perfectly ripe banana and enjoyed every bite",
                                MinDuration = 3, MaxDuration = 8, EnergyChange = 12
                            },
                            new()
                            {
                                Type = "Swinging", Description = "Swung gracefully from vine to vine", MinDuration = 5,
                                MaxDuration = 15, EnergyChange = -5
                            },
                            new()
                            {
                                Type = "Mutual Grooming", Description = "Groomed a friend and got groomed in return",
                                MinDuration = 10, MaxDuration = 25, EnergyChange = 8
                            }
                        ]
                    }
                }
            };
        }
    }
}