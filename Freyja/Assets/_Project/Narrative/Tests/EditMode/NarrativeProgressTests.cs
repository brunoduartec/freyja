using Freyja.Narrative;
using Freyja.Narrative.Editor;
using NUnit.Framework;

namespace Freyja.Narrative.Tests
{
    public sealed class NarrativeProgressTests
    {
        [TestCase(false)] [TestCase(true)]
        public void KnowledgeIsPrivateAndBothDiscoveryOrdersUnlock(bool reverse)
        {
            var state = new NarrativeProgress();
            if (reverse) { state.Discover("demo.clue.helena_birth"); state.Discover("demo.clue.boat_schedule"); }
            else { state.Discover("demo.clue.boat_schedule"); state.Discover("demo.clue.helena_birth"); }
            Assert.IsFalse(state.Knows("demo.ivo", "demo.clue.boat_schedule"));
            Assert.IsFalse(state.Knows("demo.luana", "demo.clue.boat_schedule"));
            Assert.IsTrue(state.Communicate("demo.ivo", "demo.clue.boat_schedule"));
            Assert.IsFalse(state.Knows("demo.luana", "demo.clue.boat_schedule"));
            Assert.IsTrue(state.Communicate("demo.ivo", "demo.clue.helena_birth"));
            Assert.IsTrue(state.Communicate("demo.luana", "demo.clue.boat_schedule"));
            Assert.IsFalse(state.Communicate("demo.ivo", "demo.clue.boat_schedule"));
            Assert.IsTrue(state.Knows("demo.ivo", "demo.clue.boat_schedule"));
            Assert.IsFalse(state.Knows("demo.nuno", "demo.clue.boat_schedule"));
        }

        [Test]
        public void PoliciesAndEffectsAreIdempotent()
        {
            var state = new NarrativeProgress();
            Assert.IsTrue(state.Confirm("demo.once", RepeatPolicy.Once, 8));
            Assert.IsFalse(state.Confirm("demo.once", RepeatPolicy.Once, 8));
            Assert.IsTrue(state.Confirm("demo.repeat", RepeatPolicy.Repeatable, 8));
            Assert.IsTrue(state.Confirm("demo.repeat", RepeatPolicy.Repeatable, 8));
            Assert.IsTrue(state.Confirm("demo.cooldown", RepeatPolicy.Cooldown(3), 8));
            Assert.IsFalse(state.IsAvailable("demo.cooldown", RepeatPolicy.Cooldown(3), 10));
            Assert.IsTrue(state.IsAvailable("demo.cooldown", RepeatPolicy.Cooldown(3), 11));
            Assert.IsTrue(state.ApplyEffectOnce("demo.effect"));
            Assert.IsFalse(state.ApplyEffectOnce("demo.effect"));
        }


        [Test]
        public void CsvRoundTripPreservesQuotedMultilineUnicodeAndComma()
        {
            var source = "id,text,comment\nline:demo.test,\"ação, \"\"texto\"\"\nsegunda linha\",context\n";
            var once = NarrativeCsvValidator.RoundTripCsvForTests(source);
            var twice = NarrativeCsvValidator.RoundTripCsvForTests(once);
            Assert.AreEqual(once, twice);
            StringAssert.Contains("ação", once);
            StringAssert.Contains("segunda linha", once);
        }

        [Test]
        public void CsvMergePreservesTranslationAndReportsStaleMissingOrphanAndPlaceholders()
        {
            var header = "line_id,speaker_id,node_id,source_file,kind,context,source_hash,pt-BR,en-US,translation_status\n";
            var current = header
                + "line:demo.one,,,,line,,same,Olá {name},,missing\n"
                + "line:demo.two,,,,line,,newhash,Texto fonte,,missing\n"
                + "line:demo.three,,,,line,,same,Sem parâmetro,,missing\n";
            var previous = header
                + "line:demo.one,,,,line,,same,Olá {name},Hello {name},translated\n"
                + "line:demo.two,,,,line,,oldhash,Texto antigo,Old text,translated\n"
                + "line:demo.three,,,,line,,same,Sem parâmetro,Has {other},translated\n"
                + "line:demo.removed,,,,line,,gone,Removido,Removed,translated\n";
            var merged = NarrativeCsvValidator.MergeCsvForTests(current, previous);
            StringAssert.Contains("Hello {name},translated", merged);
            StringAssert.Contains("Old text,stale", merged);
            StringAssert.Contains("Has {other},invalid_placeholders", merged);
            StringAssert.Contains("Removed,orphan", merged);
            Assert.AreEqual(merged, NarrativeCsvValidator.MergeCsvForTests(current, merged));
        }

        [Test]
        public void CsvMergeRejectsDuplicateIdsInsteadOfSilentlyDroppingRows()
        {
            var header = "line_id,speaker_id,node_id,source_file,kind,context,source_hash,pt-BR,en-US,translation_status\n";
            var duplicate = header + "line:demo.one,,,,line,,hash,Um,One,translated\nline:demo.one,,,,line,,hash,Dois,Two,translated\n";
            Assert.Throws<System.FormatException>(() => NarrativeCsvValidator.MergeCsvForTests(header, duplicate));
        }

        [Test]
        public void SaveRoundTripAndLegacyDefault()
        {
            var state = new NarrativeProgress();
            state.Discover("demo.clue.blue_ribbon", "observacao", "Manga de Ivo Batista");
            Assert.IsFalse(state.Discover("demo.clue.blue_ribbon", "observacao", "Outra fonte"));
            var restored = NarrativeProgress.FromJson(state.ToJson());
            Assert.IsTrue(restored.discovered.Contains("demo.clue.blue_ribbon"));
            Assert.AreEqual(1, restored.discoveryDetails.Count);
            Assert.AreEqual("observacao", restored.discoveryDetails[0].kind);
            Assert.AreEqual("Manga de Ivo Batista", restored.discoveryDetails[0].source);
            Assert.AreEqual(NarrativeProgress.CurrentVersion, NarrativeProgress.FromJson("{}").version);
            Assert.AreEqual(NarrativeProgress.CurrentVersion, NarrativeProgress.FromJson("").version);
        }
    }
}
