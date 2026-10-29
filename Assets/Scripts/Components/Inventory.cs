using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public event EventHandler<OnProductAddRemoveEventArgs> OnProductAdded;
    public event EventHandler<OnProductAddRemoveEventArgs> OnProductRemoved;
    public class OnProductAddRemoveEventArgs : EventArgs
    {
        public Product product;
        public int productIndex;
    }

    private const int MAX_CAPACITY = 8;

    private List<Product> productList = new(MAX_CAPACITY);
    private int currentCapacity = MAX_CAPACITY;

    public bool TryAddProduct(Product product)
    {
        if (IsFull())
        {
            return false;
        }

        int productSize = product.GetProductAsset().size;

        if (currentCapacity - productSize < 0)
        {
            return false;
        }

        currentCapacity -= productSize;

        productList.Add(product);

        OnProductAdded?.Invoke(this, new OnProductAddRemoveEventArgs { product = product, productIndex = productList.Count - 1 });

        return true;
    }

    public void RemoveProduct(int index)
    {
        currentCapacity += productList[index].GetProductAsset().size;

        Product removedProduct = productList[index];

        productList.Remove(removedProduct);

        OnProductRemoved?.Invoke(this, new OnProductAddRemoveEventArgs { product = removedProduct, productIndex = index });
    }

    public void RemoveProduct(Product product)
    {
        currentCapacity += product.GetProductAsset().size;

        productList.Remove(product);

        OnProductRemoved?.Invoke(this, new OnProductAddRemoveEventArgs { product = product, productIndex = productList.IndexOf(product) });
    }

    public Product GetProduct(int index)
    {
        return productList.ElementAt(index);
    }

    public bool IsFull()
    {
        return currentCapacity == 0;
    }

    public int GetSlotsFilledCount()
    {
        return MAX_CAPACITY - currentCapacity;
    }

    public int GetCurrentCapcity()
    {
        return currentCapacity;
    }

    public bool IsEmpty()
    {
        return currentCapacity == MAX_CAPACITY;
    }

    public int GetProductCount()
    {
        return productList.Count;
    }
}
