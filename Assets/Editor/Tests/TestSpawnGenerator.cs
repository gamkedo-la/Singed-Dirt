using NUnit.Framework;
using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace TestSinged {

    [TestFixture]
    public class TestFixedSpawnGenerator {
        [Test]
        public void TestGenerate() {
            var generator = new FixedSpawnGenerator();
            // two
            var spawnPoints = generator.Generate(2);
            Debug.Log(String.Format("spawn points -> {0}",
                String.Join(",", spawnPoints.Select(v=>v.ToString()).ToArray())));
            Assert.AreEqual(2, spawnPoints.Length);
            // twelve
            spawnPoints = generator.Generate(12);
            Debug.Log(String.Format("spawn points -> {0}",
                String.Join(",", spawnPoints.Select(v=>v.ToString()).ToArray())));
            Assert.AreEqual(12, spawnPoints.Length);
        }
    }

    [TestFixture]
    public class TestRandomSpawnGenerator {
        [Test]
        public void TestGenerate() {
            var generator = new RandomSpawnGenerator(20f, 200f, 200f);
            // two
            var spawnPoints = generator.Generate(2);
            Debug.Log(String.Format("spawn points -> {0}",
                String.Join(",", spawnPoints.Select(v=>v.ToString()).ToArray())));
            Assert.AreEqual(2, spawnPoints.Length);
            // twelve
            spawnPoints = generator.Generate(12);
            Debug.Log(String.Format("spawn points -> {0}",
                String.Join(",", spawnPoints.Select(v=>v.ToString()).ToArray())));
            Assert.AreEqual(12, spawnPoints.Length);
        }
    }

}
