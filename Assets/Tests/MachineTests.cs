using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class MachineTests
{
    // Тест 1: Машина разблокируется
    [Test]
    public void Machine_Unlock_ChangesStateToUnlocked()
    {
        // Arrange (подготовка)
        var machine = new GameObject().AddComponent<Machine>();
        Assert.AreEqual(MachineState.Locked, machine.State); // Проверяем, что изначально заблокирована

        // Act (действие)
        bool result = machine.Unlock();

        // Assert (проверка)
        Assert.IsTrue(result); // Разблокировка должна вернуться успешно
        Assert.AreEqual(MachineState.Unlocked, machine.State); // Состояние должно измениться на Unlocked
    }

    // Тест 2: Машина улучшается
    [Test]
    public void Machine_Upgrade_IncreasesLevel()
    {
        // Arrange
        var machine = new GameObject().AddComponent<Machine>();
        machine.Unlock(); // Разблокируем машину
        Assert.AreEqual(1, machine.Level); // Проверяем начальный уровень

        // Act
        bool result = machine.Upgrade();

        // Assert
        Assert.IsTrue(result); // Улучшение должно вернуться успешно
        Assert.AreEqual(2, machine.Level); // Уровень должен увеличиться до 2
    }

    // Тест 3: Машина не может улучшиться выше MaxLevel
    [Test]
    public void Machine_Upgrade_FailsIfMaxLevelReached()
    {
        // Arrange
        var machine = new GameObject().AddComponent<Machine>();
        machine.Unlock();
        for (int i = 1; i < machine.MaxLevel; i++) // Доводим до максимального уровня
        {
            machine.Upgrade();
        }
        Assert.AreEqual(machine.MaxLevel, machine.Level); // Проверяем, что уровень максимальный

        // Act
        bool result = machine.Upgrade();

        // Assert
        Assert.IsFalse(result); // Улучшение должно провалиться
        Assert.AreEqual(machine.MaxLevel, machine.Level); // Уровень не должен измениться
    }
}