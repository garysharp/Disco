using Disco.Data.Repository;
using Disco.Models.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Jobs.JobQueues
{
    internal class Cache
    {
        private ConcurrentDictionary<int, JobQueueToken> _Cache;
        private Dictionary<string, List<JobQueueToken>> _SubjectCache;

        private ReadOnlyCollection<KeyValuePair<string, string>> _Icons;
        private ReadOnlyCollection<KeyValuePair<string, string>> _IconColourCache;
        private ReadOnlyCollection<KeyValuePair<int, string>> _SlaOptions;

        public Cache(DiscoDataContext Database)
        {
            Initialize(Database);
        }

        private void Initialize(DiscoDataContext Database)
        {
            // Queues from Database
            var queues = Database.JobQueues.ToList();

            // Default System Queue
            //var defaultQueue = new JobQueue()
            //{
            //    Id = 0,
            //    Name = "Default Queue",
            //    Description = "Default system queue for orphaned jobs",
            //    Icon = "question-circle",
            //    IconColour = "F0A30A",
            //    DefaultSLAExpiry = null,
            //    Priority = JobQueuePriority.Normal,
            //    SubjectIds = null
            //};
            //queues.Add(defaultQueue);

            // Add Queues to In-Memory Cache
            this._Cache = new ConcurrentDictionary<int, JobQueueToken>(queues.Select(q => new KeyValuePair<int, JobQueueToken>(q.Id, JobQueueToken.FromJobQueue(q))));

            // Calculate Queue Subject Cache
            CalculateSubjectCache();

            #region Predefined Options
            // Icons
            if (this._Icons == null)
            {
                this._Icons = new List<KeyValuePair<string, string>>(){
                    new KeyValuePair<string, string>("ambulance" , "Ambulance"),
                    new KeyValuePair<string, string>("anchor" , "Anchor"),
                    new KeyValuePair<string, string>("android" , "Android"),
                    new KeyValuePair<string, string>("apple" , "Apple"),
                    new KeyValuePair<string, string>("archive" , "Archive"),
                    new KeyValuePair<string, string>("arrow-circle-down" , "Arrow Circle Down"),
                    new KeyValuePair<string, string>("arrow-circle-left" , "Arrow Circle Left"),
                    new KeyValuePair<string, string>("arrow-circle-right" , "Arrow Circle Right"),
                    new KeyValuePair<string, string>("arrow-circle-up" , "Arrow Circle Up"),
                    new KeyValuePair<string, string>("asterisk" , "Asterisk"),
                    new KeyValuePair<string, string>("ban" , "Ban"),
                    new KeyValuePair<string, string>("beer" , "Beer"),
                    new KeyValuePair<string, string>("bell" , "Bell"),
                    new KeyValuePair<string, string>("bolt" , "Bolt"),
                    new KeyValuePair<string, string>("bomb" , "Bomb"),
                    new KeyValuePair<string, string>("book" , "Book"),
                    new KeyValuePair<string, string>("bookmark" , "Bookmark"),
                    new KeyValuePair<string, string>("briefcase" , "Briefcase"),
                    new KeyValuePair<string, string>("bug" , "Bug"),
                    new KeyValuePair<string, string>("building-o" , "Building"),
                    new KeyValuePair<string, string>("bullhorn" , "Bullhorn"),
                    new KeyValuePair<string, string>("bullseye" , "Bullseye"),
                    new KeyValuePair<string, string>("cab" , "Cab"),
                    new KeyValuePair<string, string>("calendar" , "Calendar"),
                    new KeyValuePair<string, string>("calendar-o" , "Calendar"),
                    new KeyValuePair<string, string>("car" , "Car"),
                    new KeyValuePair<string, string>("check-circle" , "Check Circle"),
                    new KeyValuePair<string, string>("child" , "Child"),
                    new KeyValuePair<string, string>("clock-o" , "Clock"),
                    new KeyValuePair<string, string>("cloud" , "Cloud"),
                    new KeyValuePair<string, string>("coffee" , "Coffee"),
                    new KeyValuePair<string, string>("comments" , "Comments"),
                    new KeyValuePair<string, string>("compass" , "Compass"),
                    new KeyValuePair<string, string>("credit-card" , "Credit Card"),
                    new KeyValuePair<string, string>("crosshairs" , "Crosshairs"),
                    new KeyValuePair<string, string>("cube" , "Cube"),
                    new KeyValuePair<string, string>("cubes" , "Cubes"),
                    new KeyValuePair<string, string>("desktop" , "Desktop"),
                    new KeyValuePair<string, string>("dollar" , "Dollar"),
                    new KeyValuePair<string, string>("dot-circle-o" , "Dot Circle"),
                    new KeyValuePair<string, string>("envelope" , "Envelope"),
                    new KeyValuePair<string, string>("exclamation" , "Exclamation"),
                    new KeyValuePair<string, string>("eye" , "Eye"),
                    new KeyValuePair<string, string>("fax" , "Fax"),
                    new KeyValuePair<string, string>("female" , "Female"),
                    new KeyValuePair<string, string>("fighter-jet" , "Fighter Jet"),
                    new KeyValuePair<string, string>("film" , "Film"),
                    new KeyValuePair<string, string>("filter" , "Filter"),
                    new KeyValuePair<string, string>("fire" , "Fire"),
                    new KeyValuePair<string, string>("fire-extinguisher" , "Fire Extinguisher"),
                    new KeyValuePair<string, string>("flask" , "Flask"),
                    new KeyValuePair<string, string>("frown-o" , "Frown"),
                    new KeyValuePair<string, string>("gamepad" , "Gamepad"),
                    new KeyValuePair<string, string>("gift" , "Gift"),
                    new KeyValuePair<string, string>("glass" , "Glass"),
                    new KeyValuePair<string, string>("globe" , "Globe"),
                    new KeyValuePair<string, string>("graduation-cap" , "Graduation Cap"),
                    new KeyValuePair<string, string>("hand-o-down" , "Hand Down"),
                    new KeyValuePair<string, string>("hand-o-left" , "Hand Left"),
                    new KeyValuePair<string, string>("hand-o-right" , "Hand Right"),
                    new KeyValuePair<string, string>("hand-o-up" , "Hand Up"),
                    new KeyValuePair<string, string>("hdd-o" , "Hdd"),
                    new KeyValuePair<string, string>("heart" , "Heart"),
                    new KeyValuePair<string, string>("history" , "History"),
                    new KeyValuePair<string, string>("home" , "Home"),
                    new KeyValuePair<string, string>("info" , "Info"),
                    new KeyValuePair<string, string>("key" , "Key"),
                    new KeyValuePair<string, string>("keyboard-o" , "Keyboard"),
                    new KeyValuePair<string, string>("language" , "Language"),
                    new KeyValuePair<string, string>("laptop" , "Laptop"),
                    new KeyValuePair<string, string>("leaf" , "Leaf"),
                    new KeyValuePair<string, string>("legal" , "Legal"),
                    new KeyValuePair<string, string>("life-ring" , "Life Ring"),
                    new KeyValuePair<string, string>("lightbulb-o" , "Lightbulb"),
                    new KeyValuePair<string, string>("linux" , "Linux"),
                    new KeyValuePair<string, string>("location-arrow" , "Location Arrow"),
                    new KeyValuePair<string, string>("magnet" , "Magnet"),
                    new KeyValuePair<string, string>("male" , "Male"),
                    new KeyValuePair<string, string>("map-marker" , "Map Marker"),
                    new KeyValuePair<string, string>("medkit" , "Medkit"),
                    new KeyValuePair<string, string>("meh-o" , "Meh"),
                    new KeyValuePair<string, string>("microphone" , "Microphone"),
                    new KeyValuePair<string, string>("microphone-slash" , "Microphone Slash"),
                    new KeyValuePair<string, string>("minus-circle" , "Minus Circle"),
                    new KeyValuePair<string, string>("mobile" , "Mobile"),
                    new KeyValuePair<string, string>("money" , "Money"),
                    new KeyValuePair<string, string>("moon-o" , "Moon"),
                    new KeyValuePair<string, string>("music" , "Music"),
                    new KeyValuePair<string, string>("paper-plane" , "Paper Plane"),
                    new KeyValuePair<string, string>("paperclip" , "Paperclip"),
                    new KeyValuePair<string, string>("paw" , "Paw"),
                    new KeyValuePair<string, string>("pencil" , "Pencil"),
                    new KeyValuePair<string, string>("phone" , "Phone"),
                    new KeyValuePair<string, string>("picture-o" , "Picture"),
                    new KeyValuePair<string, string>("plane" , "Plane"),
                    new KeyValuePair<string, string>("power-off" , "Power Off"),
                    new KeyValuePair<string, string>("print" , "Print"),
                    new KeyValuePair<string, string>("puzzle-piece" , "Puzzle Piece"),
                    new KeyValuePair<string, string>("question" , "Question"),
                    new KeyValuePair<string, string>("question-circle" , "Question Circle"),
                    new KeyValuePair<string, string>("random" , "Random"),
                    new KeyValuePair<string, string>("recycle" , "Recycle"),
                    new KeyValuePair<string, string>("retweet" , "Retweet"),
                    new KeyValuePair<string, string>("road" , "Road"),
                    new KeyValuePair<string, string>("rocket" , "Rocket"),
                    new KeyValuePair<string, string>("shield" , "Shield"),
                    new KeyValuePair<string, string>("shopping-cart" , "Shopping Cart"),
                    new KeyValuePair<string, string>("smile-o" , "Smile"),
                    new KeyValuePair<string, string>("space-shuttle" , "Space Shuttle"),
                    new KeyValuePair<string, string>("star" , "Star"),
                    new KeyValuePair<string, string>("suitcase" , "Suitcase"),
                    new KeyValuePair<string, string>("sun-o" , "Sun"),
                    new KeyValuePair<string, string>("tablet" , "Tablet"),
                    new KeyValuePair<string, string>("tachometer" , "Tachometer"),
                    new KeyValuePair<string, string>("tasks" , "Tasks"),
                    new KeyValuePair<string, string>("thumbs-down" , "Thumbs Down"),
                    new KeyValuePair<string, string>("thumbs-o-down" , "Thumbs Down"),
                    new KeyValuePair<string, string>("thumbs-o-up" , "Thumbs Up"),
                    new KeyValuePair<string, string>("thumbs-up" , "Thumbs Up"),
                    new KeyValuePair<string, string>("thumb-tack" , "Thumb Tack"),
                    new KeyValuePair<string, string>("trash-o" , "Trash"),
                    new KeyValuePair<string, string>("trophy" , "Trophy"),
                    new KeyValuePair<string, string>("truck" , "Truck"),
                    new KeyValuePair<string, string>("umbrella" , "Umbrella"),
                    new KeyValuePair<string, string>("university" , "University"),
                    new KeyValuePair<string, string>("wheelchair" , "Wheelchair"),
                    new KeyValuePair<string, string>("windows" , "Windows"),
                    new KeyValuePair<string, string>("wrench" , "Wrench")
                }.AsReadOnly();
            }

            // Icon Colours
            if (this._IconColourCache == null)
            {
                this._IconColourCache = new List<KeyValuePair<string, string>>(){
                    new KeyValuePair<string, string>("lime" , "Lime"),
                    new KeyValuePair<string, string>("green" , "Green"),
                    new KeyValuePair<string, string>("emerald" , "Emerald"),
                    new KeyValuePair<string, string>("teal" , "Teal"),
                    new KeyValuePair<string, string>("cyan" , "Cyan"),
                    new KeyValuePair<string, string>("cobalt" , "Cobalt"),
                    new KeyValuePair<string, string>("indigo" , "Indigo"),
                    new KeyValuePair<string, string>("violet" , "Violet"),
                    new KeyValuePair<string, string>("pink" , "Pink"),
                    new KeyValuePair<string, string>("magenta" , "Magenta"),
                    new KeyValuePair<string, string>("crimson" , "Crimson"),
                    new KeyValuePair<string, string>("red" , "Red"),
                    new KeyValuePair<string, string>("orange" , "Orange"),
                    new KeyValuePair<string, string>("amber" , "Amber"),
                    new KeyValuePair<string, string>("yellow" , "Yellow"),
                    new KeyValuePair<string, string>("brown" , "Brown"),
                    new KeyValuePair<string, string>("olive" , "Olive"),
                    new KeyValuePair<string, string>("steel" , "Steel"),
                    new KeyValuePair<string, string>("mauve" , "Mauve"),
                    new KeyValuePair<string, string>("sienna" , "Sienna")
                }.AsReadOnly();
            }

            // SLA Options
            if (this._SlaOptions == null)
            {
                this._SlaOptions = new List<KeyValuePair<int, string>>()
                {
                    new KeyValuePair<int, string>(0, "<None>"),
                    new KeyValuePair<int, string>(15, "15 minutes"),
                    new KeyValuePair<int, string>(30, "30 minutes"),
                    new KeyValuePair<int, string>(60, "1 hour"),
                    new KeyValuePair<int, string>(60 * 2, "2 hours"),
                    new KeyValuePair<int, string>(60 * 4, "4 hours"),
                    new KeyValuePair<int, string>(60 * 8, "8 hours"),
                    new KeyValuePair<int, string>(60 * 24, "1 day"),
                    new KeyValuePair<int, string>(60 * 24 * 2, "2 days"),
                    new KeyValuePair<int, string>(60 * 24 * 3, "3 days"),
                    new KeyValuePair<int, string>(60 * 24 * 4, "4 days"),
                    new KeyValuePair<int, string>(60 * 24 * 5, "5 days"),
                    new KeyValuePair<int, string>(60 * 24 * 6, "6 days"),
                    new KeyValuePair<int, string>(60 * 24 * 7, "1 week"),
                    new KeyValuePair<int, string>(60 * 24 * 7 * 2, "2 weeks"),
                    new KeyValuePair<int, string>(60 * 24 * 7 * 3, "3 weeks"),
                    new KeyValuePair<int, string>(60 * 24 * 7 * 4, "4 weeks"),
                    new KeyValuePair<int, string>(60 * 24 * 7 * 4 * 2, "2 months"),
                    new KeyValuePair<int, string>(60 * 24 * 7 * 4 * 3, "3 months"),
                    new KeyValuePair<int, string>(60 * 24 * 7 * 4 * 4, "4 months"),
                    new KeyValuePair<int, string>(60 * 24 * 7 * 4 * 5, "5 months"),
                    new KeyValuePair<int, string>(60 * 24 * 7 * 4 * 6, "6 months")
                }.AsReadOnly();
            }
            #endregion
        }
        private void CalculateSubjectCache()
        {
            _SubjectCache = (from c in _Cache.Values.ToList()
                             from s in c.SubjectIds
                             group c by s into subjectId
                             select subjectId).ToDictionary(g => g.Key.ToLower(), g => g.ToList());
        }

        public ReadOnlyCollection<KeyValuePair<string, string>> Icons { get { return this._Icons; } }
        public ReadOnlyCollection<KeyValuePair<string, string>> IconColours { get { return this._IconColourCache; } }
        public ReadOnlyCollection<KeyValuePair<int, string>> SlaOptions { get { return this._SlaOptions; } }

        public JobQueueToken UpdateQueue(JobQueue JobQueue)
        {
            var token = JobQueueToken.FromJobQueue(JobQueue);
            JobQueueToken existingToken;

            if (_Cache.TryGetValue(JobQueue.Id, out existingToken))
            {
                if (_Cache.TryUpdate(JobQueue.Id, token, existingToken))
                {
                    if (existingToken.JobQueue.SubjectIds != token.JobQueue.SubjectIds)
                        CalculateSubjectCache();

                    return token;
                }
                else
                    return null;
            }
            else
            {
                if (_Cache.TryAdd(JobQueue.Id, token))
                {
                    CalculateSubjectCache();
                    return token;
                }
                else
                    return null;
            }
        }
        public bool RemoveQueue(int JobQueueId)
        {
            JobQueueToken token;
            if (_Cache.TryRemove(JobQueueId, out token))
            {
                CalculateSubjectCache();
                return true;
            }
            else
            {
                return false;
            }
        }
        public JobQueueToken GetQueue(int JobQueueId)
        {
            JobQueueToken token;
            if (_Cache.TryGetValue(JobQueueId, out token))
                return token;
            else
                return null;
        }
        public ReadOnlyCollection<JobQueueToken> GetQueues()
        {
            return _Cache.Values.ToList().AsReadOnly();
        }
        private IEnumerable<JobQueueToken> GetQueuesForSubject(string SubjectId)
        {
            List<JobQueueToken> tokens;
            if (_SubjectCache.TryGetValue(SubjectId.ToLower(), out tokens))
                return tokens;
            else
                return Enumerable.Empty<JobQueueToken>();
        }
        public ReadOnlyCollection<JobQueueToken> GetQueuesForSubject(IEnumerable<string> SubjectIds)
        {
            return SubjectIds.SelectMany(sid => GetQueuesForSubject(sid)).Distinct().ToList().AsReadOnly();
        }

        public void ReInitializeCache(DiscoDataContext Database)
        {
            Initialize(Database);
        }
    }
}
