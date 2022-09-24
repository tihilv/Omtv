using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Omtv.Engine;

namespace Omtv.Tests
{
    public class WorkflowTests
    {
        [Test]
        public async Task SimpleWorkflowTest()
        {
            var output = new TestTableOutput();
            await TableVisualizer.TransformAsync("<document><header width=\"297mm\" height=\"210mm\" name=\"Some name\"><style name=\"default\" backColor=\"white\"/><style name=\"odd\" backColor=\"gray\"/></header>" +
                                                 "<table><row><cell>v11</cell><cell>v12</cell></row><row><cell>v21</cell><cell>v22</cell></row></table></document>", output);

            Assert.That(output.ToString(), Is.EqualTo("t,r,c:v11,c:v12,/r,r,c:v21,c:v22,/r,/t"));
        }

        [Test]
        public async Task ColSpanWorkflowTest()
        {
            var output = new TestTableOutput();
            await TableVisualizer.TransformAsync("<document><header width=\"297mm\" height=\"210mm\" name=\"Some name\"><style name=\"default\" backColor=\"white\"/><style name=\"odd\" backColor=\"gray\"/></header>" +
                                                 "<table><row><cell>v11</cell><cell>v12</cell><cell>v13</cell></row><row><cell colSpan=\"2\">v21</cell><cell>v23</cell></row></table></document>", output);

            Assert.That(output.ToString(), Is.EqualTo("t,r,c:v11,c:v12,c:v13,/r,r,c:v21,c-,c:v23,/r,/t"));
        }

        [Test]
        public async Task ColSpanLastWorkflowTest()
        {
            var output = new TestTableOutput();
            await TableVisualizer.TransformAsync("<document><header width=\"297mm\" height=\"210mm\" name=\"Some name\"><style name=\"default\" backColor=\"white\"/><style name=\"odd\" backColor=\"gray\"/></header>" +
                                                 "<table><row><cell>v11</cell><cell>v12</cell><cell>v13</cell></row><row><cell>v21</cell><cell colSpan=\"2\">v22</cell></row></table></document>", output);

            Assert.That(output.ToString(), Is.EqualTo("t,r,c:v11,c:v12,c:v13,/r,r,c:v21,c:v22,c-,/r,/t"));
        }

        [Test]
        public async Task RowSpanWorkflowTest()
        {
            var output = new TestTableOutput();
            await TableVisualizer.TransformAsync("<document><header width=\"297mm\" height=\"210mm\" name=\"Some name\"><style name=\"default\" backColor=\"white\"/><style name=\"odd\" backColor=\"gray\"/></header>" +
                                                 "<table><row><cell>v11</cell><cell rowSpan=\"2\">v12</cell><cell>v13</cell></row><row><cell>v21</cell><cell>v23</cell></row></table></document>", output);

            Assert.That(output.ToString(), Is.EqualTo("t,r,c:v11,c:v12,c:v13,/r,r,c:v21,c-,c:v23,/r,/t"));
        }

        [Test]
        public async Task SpanIntersectionExceptionTest()
        {
            var output = new TestTableOutput();
            try
            {
                await TableVisualizer.TransformAsync("<document><header width=\"297mm\" height=\"210mm\" name=\"Some name\"><style name=\"default\" backColor=\"white\"/><style name=\"odd\" backColor=\"gray\"/></header>" +
                                                     "<table><row><cell>v11</cell><cell rowSpan=\"2\">v12</cell><cell>v13</cell></row><row><cell colSpan=\"2\">v21</cell><cell>v23</cell></row></table></document>", output);

                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // ok
            }
        }
        
        [Test]
        public async Task StyleTableTest()
        {
            var output = new TestTableOutput();
            await TableVisualizer.TransformAsync("<document><header width=\"297mm\" height=\"210mm\" name=\"Some name\"><style name=\"default\" backColor=\"white\"/><style name=\"odd\" backColor=\"gray\"/><style name=\"even\" backColor=\"green\"/></header>" +
                                                 "<table><row><cell>v11</cell><cell rowSpan=\"2\">v12</cell><cell>v13</cell></row><row><cell>v21</cell><cell>v23</cell></row></table></document>", output);

            Assert.That(output.Styles.Count, Is.EqualTo(3));
        }
    }
}