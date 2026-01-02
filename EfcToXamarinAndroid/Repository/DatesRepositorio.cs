using EfcToXamarinAndroid.Core;
using EfcToXamarinAndroid.Core.Configs.ManagerCore;
using EfcToXamarinAndroid.Core.Filters;
using EfcToXamarinAndroid.Core.Parsers;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EfcToXamarinAndroid.Core.Repository
{
    public static class DatesRepositorio
    {
        public static List<DataItem> DataItems { get; private set; } = new List<DataItem>();
        public static int NewDataItemsCount { get; private set; }
        public static List<DataItem> NewDataItems { get; set; }//will muve  

        public static List<DataItem> Payments = new List<DataItem>();
        public static List<DataItem> Deposits = new List<DataItem>();
        public static List<DataItem> Cashs = new List<DataItem>();
        public static List<DataItem> Unreachable = new List<DataItem>();

        private static readonly string dbFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        private static readonly string fileName = "Cats.db";
        private static readonly string dbFullPath = Path.Combine(dbFolder, fileName);

        public static event EventHandler PaymentsChanged;
        public static event EventHandler DepositsChanged;
        public static event EventHandler CashsChanged;
        public static event EventHandler UnreachableChanged;
        public static async Task<bool> SetDatasFromDB()
        {
            try
            {
                if (DataItems.Count == 0)
                {
                    using (var db = new DataItemContext(dbFullPath))
                    {
                        await db.Database.MigrateAsync(); //We need to ensure the latest Migration was added. This is different than EnsureDatabaseCreated.
                        DataItems = await db.Cats.ToListAsync();
                        UpdateAutLists(DataItems);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return false;
            }
        }
        public static async Task AddDatas(List<DataItem> dataItems)
        {
            //var newDataItems = new List<DataItem>();
            var newDataItems = GetNewDatas(dataItems);
            NewDataItems = newDataItems;//will move
            try
            {
                using (var db = new DataItemContext(dbFullPath))
                {
                    await db.Database.MigrateAsync(); //We need to ensure the latest Migration was added. This is different than EnsureDatabaseCreated.
                    if (newDataItems.Count > 0)
                    {
                        await db.Cats.AddRangeAsync(newDataItems);
                        await db.SaveChangesAsync();
                        DataItems.AddRange(newDataItems);
                        UpdateAutLists(DataItems);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
        public static async Task DeleteItem(DataItem dataItem)
        {
            DataItems.Remove(dataItem);
            UpdateAutLists(DataItems);
            try
            {
                using (var db = new DataItemContext(dbFullPath))
                {
                    await db.Database.MigrateAsync(); //We need to ensure the latest Migration was added. This is different than EnsureDatabaseCreated.
                    db.Entry(dataItem).State = EntityState.Deleted;
                    await db.SaveChangesAsync();

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
        public static async Task<bool> DeleteAllItems()
        {
            try
            {
                using (var db = new DataItemContext(dbFullPath))
                {
                    foreach (var dataItem in DataItems)
                    {
                        db.Cats.Remove(dataItem);
                    }

                    await db.SaveChangesAsync();
                }
                DataItems.Clear();
                UpdateAutLists(DataItems);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return false;
            }
        }
        private static List<DataItem> GetNewDatas(List<DataItem> dataItems)
        {
            //var stopWatch = new Stopwatch();
            //stopWatch.Start();

            var newDataItems = new List<DataItem>();

            if (DataItems.Count > 0)
            {
                foreach (var item in dataItems)
                {
                    if (item.Date.Second == 0)
                    {
                        if (!DataItems.Any(x => x.HashId == item.HashId))
                            newDataItems.Add(item);
                        else
                        {
                            if (!DataItems.Where(x => x.HashId == item.HashId).Any(x => x.Sum == item.Sum))
                                if (!DataItems.Where(x => x.HashId == item.HashId).Where(x => x.Sum == item.Sum).Any(x => x.OldSum == item.Sum))
                                    newDataItems.Add(item);
                        }
                    }
                    else
                    {
                        if (!DataItems.Any(x => x.Date == item.Date))
                            newDataItems.Add(item);
                        else
                        {
                            if (!DataItems.Where(x => x.Date == item.Date).Any(x => x.Sum == item.Sum))
                                if (!DataItems.Where(x => x.Date == item.Date).Any(x => x.OldSum == item.Sum))
                                    newDataItems.Add(item);
                        }
                    }
                }
            }
            else
                newDataItems = dataItems;
            //stopWatch.Stop();
            //Console.WriteLine(stopWatch.Elapsed);
            newDataItems.ForEach(x => x.IsNewDataItem = true);
            return newDataItems;
        }
        public static void ClearNewDatasRemuveAll()
        {
            NewDataItems.Clear();
        }

        public static void UnCheckNewItems(int tabIndex)
        {
            switch (tabIndex)
            {
                case 0:
                    Payments.Where(x => x.IsNewDataItem == true).ToList().ForEach(x => x.IsNewDataItem = false);
                    PaymentsChanged?.Invoke(Payments, EventArgs.Empty);
                    break;
                case 1:
                    Deposits.Where(x => x.IsNewDataItem == true).ToList().ForEach(x => x.IsNewDataItem = false);
                    DepositsChanged?.Invoke(Deposits, EventArgs.Empty);
                    break;
                case 2:
                    Cashs.Where(x => x.IsNewDataItem == true).ToList().ForEach(x => x.IsNewDataItem = false);
                    CashsChanged?.Invoke(Cashs, EventArgs.Empty);
                    break;
                case 3:
                    Unreachable.Where(x => x.IsNewDataItem == true).ToList().ForEach(x => x.IsNewDataItem = false);
                    UnreachableChanged?.Invoke(Unreachable, EventArgs.Empty);
                    break;
                default:
                    break;
            }
        }
        private static void UpdateAutLists(List<DataItem> dataItems)
        {
            //  var stopWatch = new Stopwatch();
            //  stopWatch.Start();

            List<DataItem> ordetDataItems = dataItems.OrderBy(x => x.Date).Reverse().ToList();
            if (!GetPayments(ordetDataItems).SequenceEqual(Payments))
            {
                Payments.Clear();
                Payments.AddRange(GetPayments(ordetDataItems));
                PaymentsChanged?.Invoke(Payments, EventArgs.Empty);
            }
            if (!GetDeposits(ordetDataItems).SequenceEqual(Deposits))
            {
                Deposits.Clear();
                Deposits.AddRange(GetDeposits(ordetDataItems));
                DepositsChanged?.Invoke(Deposits, EventArgs.Empty);
            }
            if (!GetCashs(ordetDataItems).SequenceEqual(Cashs))
            {
                Cashs.Clear();
                Cashs.AddRange(GetCashs(ordetDataItems));
                CashsChanged?.Invoke(Cashs, EventArgs.Empty);
            }
            if (!GetUnreachable(ordetDataItems).SequenceEqual(Unreachable))
            {
                Unreachable.Clear();
                Unreachable.AddRange(GetUnreachable(ordetDataItems));
                UnreachableChanged?.Invoke(Unreachable, EventArgs.Empty);
            }

            // stopWatch.Stop();
            // Console.WriteLine(stopWatch.Elapsed);
        }
        private static void UpdateItemsViews(List<DataItem> dataItems)
        {
            List<DataItem> ordetDataItems = dataItems.OrderBy(x => x.Date).Reverse().ToList();
            if (dataItems.Any(x => x.OperacionTyp == OperacionTyps.OPLATA))
            {
                PaymentsChanged?.Invoke(Payments, EventArgs.Empty);
            }
            if (dataItems.Any(x => x.OperacionTyp == OperacionTyps.ZACHISLENIE))
            {
                DepositsChanged?.Invoke(Payments, EventArgs.Empty);
            }
            if (dataItems.Any(x => x.OperacionTyp == OperacionTyps.NALICHNYE))
            {
                PaymentsChanged?.Invoke(Payments, EventArgs.Empty);
            }
            if (dataItems.Any(x => x.OperacionTyp == OperacionTyps.UNREACHABLE))
            {
                UnreachableChanged?.Invoke(Payments, EventArgs.Empty);
            }
        }
        public static async Task UpdateItemValue(int id, DataItem newValue)
        {
            var item = DataItems.SingleOrDefault(x => x.Id == id);
            if (item.Sum > newValue.Sum)
            {
                // !! исправить!!! item.Sum = item.Sum - newValue.Sum;   //!! Если данную ф-ю разместить до установки нового значения код работает не верно
                // и переменной олд сум присваевается новое значение суммы, а не старое

                item.SetNewValues(item);
                item.Sum = item.Sum - newValue.Sum;
                while (DatesRepositorio.DataItems.Any(x => x.Date == newValue.Date))
                    newValue.Date = newValue.Date.Second == 59 ?
                        newValue.Date.AddSeconds(-59) :
                        newValue.Date.AddSeconds(1); //создаем потомка с различающимся временем в секундах
                                                     //HashId потомка остается таким же как у родителя
                                                     //при добавлении данных возможно повторное добовление родителя
                newValue.ParentId = item.Id;
                AddDatas(new List<DataItem> { newValue });
            }
            else
                item?.SetNewValues(newValue);
            try
            {
                using (var db = new DataItemContext(dbFullPath))
                {
                    var result = db.Cats.SingleOrDefault(x => x.Id == id);
                    if (result != null)
                    {
                        result.SetNewValues(item);
                        await db.SaveChangesAsync();
                    }
                }
                UpdateItemsViews(new[] { item }.ToList());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        public static async Task<IEnumerable<DataItem>> GetDataItems(GetItemsRequest getItemsRequest)
        {
            try
            {
                using (var db = new DataItemContext(dbFullPath))
                {
                    var query = db.Cats.AsQueryable();

                    // Этот фильтр будет применяться только если тип операции указан в  

                    if (getItemsRequest.Typ != null)
                    {
                        if (getItemsRequest.Typ != OperacionTyps.None)
                            query = query.Where(x => x.OperacionTyp == getItemsRequest.Typ);
                    }


                    // Сортируем по дате (сначала новые), пропускаем нужное количество и

                    return await query
                        .OrderByDescending(x => x.Date)
                        .Skip(getItemsRequest.StartIndex)
                        .Take(getItemsRequest.Count)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return new List<DataItem>();
            }
        }

        public static async Task<DataItem> GetDataItem(int id)
        {
            return DataItems.SingleOrDefault(x => x.Id == id);
        }

        public static List<DataItem> GetPayments(List<DataItem> dataItems)
        {
            MccConfigurationManager mccManager = MccConfigurationManager.ConfigManager;
            var codes = mccManager.MccConfigurationFromJson;
            var sdf = dataItems.Where(x => x.OperacionTyp == OperacionTyps.OPLATA).Select(x => x.MccDeskription = codes.Keys.Contains(x.MCC) ? codes[x.MCC] : null).ToList();
            ////return dataItems.Where(x => x.OperacionTyp == OperacionTyps.OPLATA).ToList();
            return dataItems.Where(x => x.OperacionTyp == OperacionTyps.OPLATA).ToList();
        }
        public static List<DataItem> GetDeposits(List<DataItem> dataItems)
        {
            return dataItems.Where(x => x.OperacionTyp == OperacionTyps.ZACHISLENIE).ToList();
        }
        public static List<DataItem> GetCashs(List<DataItem> dataItems)
        {
            return dataItems.Where(x => x.OperacionTyp == OperacionTyps.NALICHNYE).ToList();
        }
        public static List<DataItem> GetUnreachable(List<DataItem> dataItems)
        {
            return dataItems?.Where(x => x.OperacionTyp == OperacionTyps.UNREACHABLE).ToList();
        }
        public static List<string> GetTags()
        {
            Dictionary<string, int> tagsWithMass = new Dictionary<string, int>();
            var subTtags = DatesRepositorio.DataItems.Select(x => x.Title).OfType<String>().Where(x => x != "").Select(x => x.Split(" "));
            foreach (var tags in subTtags)
            {
                foreach (var tag in tags)
                {
                    if (!tagsWithMass.TryAdd(tag, 1))
                        tagsWithMass[tag]++;
                }
            }
            return tagsWithMass.OrderBy(x => x.Value).Reverse().Select(x => x.Key).ToList();
        }



        #region Filter
        public static MFilter MFilter
        {
            get
            {
                MFilter mFilter = new MFilter(DataItems);
                mFilter.FiltredClose += MFilter_Filtred;
                return mFilter;
            }
        }
        private static void MFilter_Filtred(object sender)
        {
            UpdateAutLists(((MFilter)sender).OutDataItems);
        }

        public static void SearchDuplicate()
        {
            DuplicatesSearcher searcher = new DuplicatesSearcher(DatesRepositorio.DataItems);
            var duplicates = searcher.SearcDuplicates();
            //UpdateAutLists(duplicates);

            UpdateItemsViews(duplicates);
        }
        #endregion
    }
}