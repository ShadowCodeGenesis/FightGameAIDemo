﻿using FluentBehaviourTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using FluentBehaviourTree;
using Fluent_behavior_tree.mehNodes;
using Fluent_behavior_tree;

namespace tests
{
    public class BehaviourTreeBuilderTests
    {
        MyTreeBuilder testObject;

        void Init()
        {
            testObject = new MyTreeBuilder();
        }

        [Fact]
        public void cant_create_a_behaviour_tree_with_zero_nodes()
        {
            Init();

            Assert.Throws<ApplicationException>(() =>
                {
                    testObject.Build();
                }
            );

        }

        [Fact]
        public void cant_create_an_unested_action_node()
        {
            Init();

            Assert.Throws<ApplicationException>(() =>
                {
                    testObject
                         .Do("some-node-1", t => MyBehaviourTreeStatus.Running)
                         .Build();
                }
            );
        }

        [Fact]
        public void can_create_inverter_node()
        {
            Init();

            var node = testObject
                .Inverter("some-inverter")
                    .Do("some-node", t => MyBehaviourTreeStatus.Success)
                .End()
                .Build();

            Assert.IsType<InverterNode>(node);
            Assert.Equal(MyBehaviourTreeStatus.Failure, node.Tick(new MyTimeData()));
        }

        [Fact]
        public void cant_create_an_unbalanced_behaviour_tree()
        {
            Init();

            Assert.Throws<ApplicationException>(() =>
            {
                testObject
                    .Inverter("some-inverter")
                    .Do("some-node", t => MyBehaviourTreeStatus.Success)
                .Build();
            });
        }

        [Fact]
        public void condition_is_syntactic_sugar_for_do()
        {
            Init();

            var node = testObject
                .Inverter("some-inverter")
                    .Condition("some-node", t => true)
                .End()
                .Build();

            Assert.IsType<InverterNode>(node);
            Assert.Equal(MyBehaviourTreeStatus.Failure, node.Tick(new MyTimeData()));
        }

        [Fact]
        public void can_invert_an_inverter()
        {
            Init();

            var node = testObject
                .Inverter("some-inverter")
                    .Inverter("some-other-inverter")
                        .Do("some-node", t => MyBehaviourTreeStatus.Success)
                    .End()
                .End()
                .Build();

            Assert.IsType<InverterNode>(node);
            Assert.Equal(MyBehaviourTreeStatus.Success, node.Tick(new MyTimeData()));
        }

        [Fact]
        public void adding_more_than_a_single_child_to_inverter_throws_exception()
        {
            Init();

            Assert.Throws<ApplicationException>(() =>
            {
                testObject
                    .Inverter("some-inverter")
                        .Do("some-node", t => MyBehaviourTreeStatus.Success)
                        .Do("some-other-node", t => MyBehaviourTreeStatus.Success)
                    .End()
                    .Build();
            });
        }

        [Fact]
        public void can_create_a_sequence()
        {
            Init();

            var invokeCount = 0;

            var sequence = testObject
                .Sequence("some-sequence")
                    .Do("some-action-1", t => 
                    {
                        ++invokeCount;
                        return MyBehaviourTreeStatus.Success;
                    })
                    .Do("some-action-2", t =>
                    {
                        ++invokeCount;
                        return MyBehaviourTreeStatus.Success;
                    })
                .End()
                .Build();

            Assert.IsType<SequenceNode>(sequence);
            Assert.Equal(MyBehaviourTreeStatus.Success, sequence.Tick(new MyTimeData()));
            Assert.Equal(2, invokeCount);
        }

        [Fact]
        public void can_create_parallel()
        {
            Init();

            var invokeCount = 0;

            var parallel = testObject
                .Parallel("some-parallel", 2, 2)
                    .Do("some-action-1", t =>
                    {
                        ++invokeCount;
                        return MyBehaviourTreeStatus.Success;
                    })
                    .Do("some-action-2", t =>
                    {
                        ++invokeCount;
                        return MyBehaviourTreeStatus.Success;
                    })
                .End()
                .Build();

            Assert.IsType<ParallelNode>(parallel);
            Assert.Equal(MyBehaviourTreeStatus.Success, parallel.Tick(new MyTimeData()));
            Assert.Equal(2, invokeCount);
        }

        [Fact]
        public void can_create_selector()
        {
            Init();

            var invokeCount = 0;

            var parallel = testObject
                .Selector("some-selector")
                    .Do("some-action-1", t =>
                    {
                        ++invokeCount;
                        return MyBehaviourTreeStatus.Failure;
                    })
                    .Do("some-action-2", t =>
                    {
                        ++invokeCount;
                        return MyBehaviourTreeStatus.Success;
                    })
                .End()
                .Build();

            Assert.IsType<SelectorNode>(parallel);
            Assert.Equal(MyBehaviourTreeStatus.Success, parallel.Tick(new MyTimeData()));
            Assert.Equal(2, invokeCount);
        }

        [Fact]
        public void can_splice_sub_tree()
        {
            Init();

            var invokeCount = 0;

            var spliced = testObject
                .Sequence("spliced")
                    .Do("test", t =>
                    {
                        ++invokeCount;
                        return MyBehaviourTreeStatus.Success;
                    })
                .End()
                .Build();

            var tree = testObject
                .Sequence("parent-tree")
                    .Splice(spliced)                    
                .End()
                .Build();

            tree.Tick(new MyTimeData());

            Assert.Equal(1, invokeCount);
        }

        [Fact]
        public void splicing_an_unnested_sub_tree_throws_exception()
        {
            Init();

            var invokeCount = 0;

            var spliced = testObject
                .Sequence("spliced")
                    .Do("test", t =>
                    {
                        ++invokeCount;
                        return MyBehaviourTreeStatus.Success;
                    })
                .End()
                .Build();

            Assert.Throws<ApplicationException>(() =>
            {
                testObject
                    .Splice(spliced);
            });
        }
    }
}
