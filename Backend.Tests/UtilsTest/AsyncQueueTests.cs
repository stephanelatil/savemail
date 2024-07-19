namespace Backend.Tests;
using Backend.Utils;

public class AsyncQueueTest
{
    [Fact]
    public async Task Test_QueueEnqueueDequeue_NoException()
    {
        var queue = new AsyncQueue<int>();
        queue.Enqueue(1);

        Assert.Equal(1, queue.Count);

        int result = await queue.DequeueAsync();
        Assert.Equal(1, result);
    }

    [Theory]
    [InlineData(123)]
    [InlineData(1,3,5)]
    [InlineData(0,9,66,5,4,3,3)]
    public async Task Test_QueueEnqueueDequeue_Ordering(params int[] numbers)
    {
        var queue = new AsyncQueue<int>();
        foreach(int number in numbers)
            queue.Enqueue(number);
        
        foreach(int number in numbers)
            Assert.Equal(number, await queue.DequeueAsync());
    }


    [Theory]
    [InlineData(123)]
    [InlineData(1,3,5)]
    [InlineData(0,9,66,5,4,3,3)]
    public async Task Test_QueueEnqueueDequeue_Count(params int[] numbers)
    {
        var queue = new AsyncQueue<int>();
        foreach(int number in numbers)
            queue.Enqueue(number);

        int expectedCount = numbers.Length;
        while(expectedCount > 0)
        {
            Assert.Equal(expectedCount, queue.Count);
            _ = await queue.DequeueAsync();
            expectedCount--;
        }
    }

    [Fact]
    public async Task Test_Queue_BlockingIfEmpty()
    {
        var queue = new AsyncQueue<int>();

        var dequeue = async () => await queue.DequeueAsync(10);
        
        await Assert.ThrowsAsync<TimeoutException>(dequeue);
    }
}