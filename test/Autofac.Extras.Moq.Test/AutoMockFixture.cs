using System;
using Autofac.Features.Indexed;
using Moq;
using Xunit;

namespace Autofac.Extras.Moq.Test
{
    public class AutoMockFixture
    {
        [Fact]
        public void AbstractDependencyIsFulfilled()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var component = mock.Create<TestComponentRequiringAbstractClassA>();
                Assert.Equal(
                    mock.Mock<AbstractClassA>().Object,
                    component.InstanceOfAbstractClassA);
            }
        }

        [Fact]
        public void RegularClassDependencyIsFulfilled()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var component = mock.Create<TestComponentRequiringClassA>();
                Assert.Equal(
                    mock.Mock<ClassA>().Object,
                    component.InstanceOfClassA);
            }
        }

        [Fact]
        public void DefaultConstructorIsLoose()
        {
            using (var mock = AutoMock.GetLoose())
            {
                RunWithSingleSetupationTest(mock);
            }
        }

        [Fact]
        public void DefaultConstructorWorksWithAllTests()
        {
            using (var mock = AutoMock.GetLoose())
            {
                RunTest(mock);
            }
        }

        [Fact]
        public void GetFromRepositoryUsesLooseBehaviorSetOnRepository()
        {
            using (var mock = AutoMock.GetFromRepository(new MockRepository(MockBehavior.Loose)))
            {
                RunWithSingleSetupationTest(mock);
            }
        }

        [Fact]
        public void GetFromRepositoryUsesStrictBehaviorSetOnRepository()
        {
            using (var mock = AutoMock.GetFromRepository(new MockRepository(MockBehavior.Strict)))
            {
                Assert.Throws<MockException>(() => RunWithSingleSetupationTest(mock));
            }
        }

        [Fact]
        public void LooseWorksWithUnmetSetupations()
        {
            using (var loose = AutoMock.GetLoose())
            {
                RunWithSingleSetupationTest(loose);
            }
        }

        [Fact]
        public void NormalSetupationsAreNotVerifiedByDefault()
        {
            using (var mock = AutoMock.GetLoose())
            {
                SetUpSetupations(mock);
            }
        }

        [Fact]
        public void ProperInitializationIsPerformed()
        {
            AssertProperties(AutoMock.GetLoose());
            AssertProperties(AutoMock.GetStrict());
        }

        [Fact]
        public void ProvideImplementation()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var serviceA = mock.Provide<IServiceA, ServiceA>();

                Assert.NotNull(serviceA);
                Assert.False(serviceA is IMocked<IServiceA>);
            }
        }

        [Fact]
        public void ProvideInstance()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var mockA = new Mock<IServiceA>();
                mockA.Setup(x => x.RunA());
                mock.Provide(mockA.Object);

                var component = mock.Create<TestComponent>();
                component.RunAll();

                mockA.VerifyAll();
            }
        }

        [Fact]
        public void ProvideKeyedImplementation()
        {
            using (AutoMock mock = AutoMock.GetLoose())
            {
                var serviceA = mock.ProvideKeyed<IServiceA, ServiceA>("A");
                var serviceA2 = mock.ProvideKeyed<IServiceA, ServiceA2>("A2");

                Assert.NotNull(serviceA);
                Assert.NotNull(serviceA2);
                Assert.IsType<ServiceA>(serviceA);
                Assert.IsType<ServiceA2>(serviceA2);
            }
        }

        [Fact]
        public void ProvideKeyedInstance()
        {
            using (AutoMock mock = AutoMock.GetLoose())
            {
                var mockA = new Mock<IServiceA>();
                mockA.Setup(a => a.RunA());
                mock.ProvideKeyed("A", mockA.Object);

                var mockA2 = new Mock<IServiceA>();
                mockA2.Setup(a => a.RunA());
                mock.ProvideKeyed("A2", mockA2.Object);

                var component = mock.Create<TestComponentRequiringIIndex>();
                component.RunAll();

                mockA.VerifyAll();
                mockA2.VerifyAll();


            }
        }

        [Fact]
        public void StrictWorksWithAllSetupationsMet()
        {
            using (var strict = AutoMock.GetStrict())
            {
                RunTest(strict);
            }
        }

        [Fact]
        public void UnmetSetupationWithStrictMocksThrowsException()
        {
            using (var mock = AutoMock.GetStrict())
            {
                Assert.Throws<MockException>(() => RunWithSingleSetupationTest(mock));
            }
        }

        [Fact]
        public void UnmetVerifiableSetupationsCauseExceptionByDefault()
        {
            Assert.Throws<MockException>(() =>
                {
                    using (var mock = AutoMock.GetLoose())
                    {
                        SetUpVerifableSetupations(mock);
                    }
                });
        }

        [Fact]
        public void VerifyAllSetTrue_SetupationsAreVerified()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.VerifyAll = true;
                RunTest(mock);
            }
        }

        [Fact]
        public void VerifyAllSetTrue_UnmetSetupationsCauseException()
        {
            Assert.Throws<MockException>(() =>
                {
                    using (var mock = AutoMock.GetLoose())
                    {
                        mock.VerifyAll = true;
                        SetUpSetupations(mock);
                    }
                });
        }

        private static void AssertProperties(AutoMock mock)
        {
            Assert.NotNull(mock.Container);
            Assert.NotNull(mock.MockRepository);
        }

        private static void RunTest(AutoMock mock)
        {
            SetUpSetupations(mock);

            var component = mock.Create<TestComponent>();
            component.RunAll();
        }

        private static void RunWithSingleSetupationTest(AutoMock mock)
        {
            mock.Mock<IServiceB>().Setup(x => x.RunB());

            var component = mock.Create<TestComponent>();
            component.RunAll();
        }

        private static void SetUpSetupations(AutoMock mock)
        {
            mock.Mock<IServiceB>().Setup(x => x.RunB());
            mock.Mock<IServiceA>().Setup(x => x.RunA());
        }

        private static void SetUpVerifableSetupations(AutoMock mock)
        {
            mock.Mock<IServiceB>().Setup(x => x.RunB()).Verifiable();
            mock.Mock<IServiceA>().Setup(x => x.RunA()).Verifiable();
        }

        public interface IServiceA
        {
            void RunA();
        }

        public interface IServiceB
        {
            void RunB();
        }

        public abstract class AbstractClassA
        {
        }

        // ReSharper disable once ClassNeverInstantiated.Global
        public class ClassA : AbstractClassA
        {
        }

        // ReSharper disable once ClassNeverInstantiated.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public class ServiceA : IServiceA
        {
            public void RunA()
            {
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public class ServiceA2 : IServiceA
        {
            public void RunA()
            {
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public sealed class TestComponent
        {
            private readonly IServiceA _serviceA;

            private readonly IServiceB _serviceB;

            public TestComponent(IServiceA serviceA, IServiceB serviceB)
            {
                this._serviceA = serviceA;
                this._serviceB = serviceB;
            }

            public void RunAll()
            {
                this._serviceA.RunA();
                this._serviceB.RunB();
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public sealed class TestComponentRequiringAbstractClassA
        {
            public TestComponentRequiringAbstractClassA(AbstractClassA abstractClassA)
            {
                this.InstanceOfAbstractClassA = abstractClassA;
            }

            public AbstractClassA InstanceOfAbstractClassA { get; }
        }

        // ReSharper disable once ClassNeverInstantiated.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public sealed class TestComponentRequiringClassA
        {
            public TestComponentRequiringClassA(ClassA classA)
            {
                this.InstanceOfClassA = classA;
            }

            public ClassA InstanceOfClassA { get; }
        }

        public sealed class TestComponentRequiringIIndex
        {
            private readonly IServiceA _a;
            private readonly IServiceA _a2;

            public TestComponentRequiringIIndex(IIndex<object, IServiceA> keyedServices)
            {
                _a = keyedServices["A"];
                _a2 = keyedServices["A2"];
            }

            public void RunAll()
            {
                this._a.RunA();
                this._a2.RunA();
            }
        }
    }
}
