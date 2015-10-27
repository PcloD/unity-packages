﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using N;
using N.Tests;

namespace N {

  /// Runner for all the various tests
  public class TestRunner : MonoBehaviour {

    [Tooltip("Run tests continually, this often, if the value is set.")]
    public float interval;

    [Tooltip("Filter test class using this regex if desired, eg. MyPackage.*")]
    public string filter;

    [Tooltip("Enable and disable tests using this")]
    public new bool enabled = true;

    /// Elapsed time since last test run
    private float elapsed;

    /// Run all tests
    public void Start() {
      TestRunner.Run(filter);
    }

    /// If the interval is set, run tests after an interval.
    /// This lets you hit play and tab out, then tab back to get new results.
    public void Update() {
      if (!enabled) {
        return;
      }
      if (interval > 0.0f) {
        elapsed += Time.deltaTime;
        if (elapsed > interval) {
          TestRunner.Run(filter);
          elapsed = 0.0f;
        }
      }
    }

    /// Run all tests regardless of context
    public static void Run() {
      TestRunner.Run(null);
    }

    /// Run all tests regardless of context
    public static void Run(string filter) {
      try {
        // Make output parsable
        N.Console.Clear();
        N.Console.Log("START TESTS");

        // Read arguments
        if (string.IsNullOrEmpty(filter)) {
          var args = new N.Env.Arguments();
          filter = args.Named("filterTests");
          if (filter != null) {
            N.Console.Log("Filtering tests to: " + filter);
          }
        }

        // Find types
        // If a filter was supplied, use the filter.
        var items = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
        var tests = new List<TestSuite>();
        for (var ts = 0; ts < items.Length; ++ts) {
          if (items[ts].IsSubclassOf(typeof(TestSuite))) {
            bool isMatch = false;
            if (!string.IsNullOrEmpty(filter)) {
              var name = items[ts].FullName;
              if (Regex.IsMatch(name, filter)) {
                isMatch = true;
              }
            }
            else {
              isMatch = true;
            }
            if (isMatch) {
              tests.Add(System.Activator.CreateInstance(items[ts]) as TestSuite);
            }
          }
        }

        N.Console.Log(DateTime.Now.ToString());

        var all = 0;
        for (var i = 0; i < tests.Count; ++i) {
          try {
            N.Console.DEBUG = false;
            tests[i].Run();
            all += tests[i].total;
          }
          catch(Exception e) {
            N.Console.Log(e);
          }
        }

        N.Console.DEBUG = false;
        for (var j = 0; j < tests.Count; ++j) {
          var t = tests[j];
          if (t.passed.Count == t.total) {
            N.Console.Log("- Test: " + t + ": " + t.passed.Count + "/" + t.total);
          }
          else {
            N.Console.Log("! Test: " + t + ": " + t.passed.Count + "/" + t.total);
          }
        }

        for (var k = 0; k < tests.Count; ++k) {
          var t2 = tests[k];
          for (var l = 0; l < t2.failed.Count; ++l) {
            var f2 = t2.failed[l];
            N.Console.Error(f2.error.Message);
            foreach (var line in f2.error.StackTrace.Split('\n')) {
              N.Console.Error(line);
            }
          }
        }

        // Make output parsable
        N.Console.Log("END TESTS");
      }
      catch(Exception e) {
        Debug.Log(e);
      }
    }
  }
}
