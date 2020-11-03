namespace TakeOutFood
{
    using System;
    using System.Collections.Generic;

    public class App
    {
        private IItemRepository itemRepository;
        private ISalesPromotionRepository salesPromotionRepository;

        public App(IItemRepository itemRepository, ISalesPromotionRepository salesPromotionRepository)
        {
            this.itemRepository = itemRepository;
            this.salesPromotionRepository = salesPromotionRepository;
        }

        public string BestCharge(List<string> inputs)
        {
            //TODO: write code here
            string[] input = new string[] { "ITEM0001 x 1", "ITEM0013 x 2", "ITEM0022 x 1" };
            List<Item> foodList = itemRepository.FindAll();
            List<SalesPromotion> promotionList = salesPromotionRepository.FindAll();
            List<string> usePromotionList = new List<string>();
            bool haveSalesPromotion = false;
            List<FoodOrder> foodOrders = GenerateFoodOrder(foodList, input, promotionList, usePromotionList, ref haveSalesPromotion);
            double originalTotalPrice = 0;
            double totalPrice = 0;
            string output = "============= Order details =============\n";
            foreach (FoodOrder singleFoodOrder in foodOrders)
            {
                originalTotalPrice += singleFoodOrder.OriginalSutTotal;
                totalPrice += singleFoodOrder.SutTotal;
                output += singleFoodOrder.Name + " x " + singleFoodOrder.Count + " = " + singleFoodOrder.OriginalSutTotal + " yuan\n";
            }
            output += "-----------------------------------\n";
            if (haveSalesPromotion)
            {
                output += "Promotion used:\n"+ "Half price for certain dishes (";
                foreach (string useName in usePromotionList)
                {
                    output += useName + ", ";
                }
                output = output.Substring(0, output.Length - 2);
                output += ")";
                double savingMoney = originalTotalPrice - totalPrice;
                output += ", saving " + savingMoney + " yuan\n";
                output += "-----------------------------------\n";
            }
            output += "Total：" + totalPrice + " yuan\n" + "===================================";
            return output;
        }

        private List<FoodOrder> GenerateFoodOrder(List<Item> foodList, string[] input, List<SalesPromotion> promotionList, List<string> usePromotionList, ref bool haveSalesPromotion)
        {
            List<FoodOrder> foodOrders = new List<FoodOrder>();
            for (int i = 0; i < input.Length; i++)
            {
                FoodOrder thisFoodOrder = new FoodOrder();
                string foodId = input[i].Substring(0, 8);
                thisFoodOrder.Id = foodId;
                int count = Convert.ToInt32(input[i].Remove(0, 11));
                thisFoodOrder.Count = count;
                foreach (Item foodItem in foodList)
                {
                    if (foodItem.Id.ToLower() == foodId.ToLower())
                    {
                        thisFoodOrder.Name = foodItem.Name;
                        double discountRate = 1;
                        if (InSalesPromotionRepository(foodItem.Id, promotionList))
                        {
                            haveSalesPromotion = true;
                            usePromotionList.Add(foodItem.Name);
                            discountRate = 0.5;
                        }
                        thisFoodOrder.Price = foodItem.Price;
                        thisFoodOrder.FinalPrice = foodItem.Price * discountRate;
                    }
                }
                thisFoodOrder.OriginalSutTotal = thisFoodOrder.Count * thisFoodOrder.Price;
                thisFoodOrder.SutTotal = thisFoodOrder.Count * thisFoodOrder.FinalPrice;
                foodOrders.Add(thisFoodOrder);
            }
            return foodOrders;
        }

        private bool InSalesPromotionRepository(string foodId, List<SalesPromotion> promotionList)
        {
            foreach (SalesPromotion promotion in promotionList)
            {
                foreach (string singleItem in promotion.RelatedItems)
                {
                    if (foodId.ToLower() == singleItem.ToLower())
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public class FoodOrder
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public double FinalPrice { get; set; }
        public int Count { get; set; }
        public double OriginalSutTotal { get; set; }
        public double SutTotal { get; set; }
    }

    public class ItemRepository : IItemRepository
    {
        private List<Item> foodList;
        public ItemRepository()
        {
            foodList.Add(new Item("ITEM0001", "Braised chicken", 18.00));
            foodList.Add(new Item("ITEM0013", "Chinese hamburger", 6.00));
            foodList.Add(new Item("ITEM0022", "Cold noodles", 8.00));
        }
        public List<Item> FindAll()
        {
            return foodList;
        }
    }

    public class SalesPromotionRepository : ISalesPromotionRepository
    {
        private List<SalesPromotion> promotionList = new List<SalesPromotion>() { new SalesPromotion("Half price", "DisplayName", new List<string>() { "ITEM0001", "ITEM0022" }) };

        public List<SalesPromotion> FindAll()
        {
            return promotionList;
        }
    }
}
