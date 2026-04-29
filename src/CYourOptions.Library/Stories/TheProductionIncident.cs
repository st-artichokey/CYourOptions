using CYourOptions.Library.Models;

namespace CYourOptions.Library.Stories;

/// <summary>
/// "The Production Incident" — a branching story about a developer's Monday morning
/// when everything goes wrong. 30 nodes, 6 distinct endings.
/// </summary>
public static class TheProductionIncident
{
    public const string StartNodeId = "monday_morning";

    public static List<DecisionNode> GetNodes() =>
    [
        // === ACT 1: The Alert ===

        new DecisionNode
        {
            Id = "monday_morning",
            Title = "Monday Morning",
            Text = "You're sipping coffee at your desk when Slack explodes. PagerDuty fires. The #incidents channel lights up: 'CRITICAL — payments service returning 500s in production.' Your manager posts: 'All hands on deck.' Your terminal is open. What do you do?",
            Choices =
            [
                new Choice { Label = "Check the logs immediately", NextNodeId = "check_logs" },
                new Choice { Label = "Look at the deploy history", NextNodeId = "deploy_history" },
                new Choice { Label = "Ask in the channel what changed", NextNodeId = "ask_channel" }
            ]
        },

        new DecisionNode
        {
            Id = "check_logs",
            Title = "Reading the Logs",
            Text = "You pull up Datadog. The error rate spiked at 6:47 AM — a NullReferenceException in PaymentProcessor.ProcessCharge(). The stack trace points to a field that should have been populated by a config value. The config service shows healthy. Interesting.",
            Choices =
            [
                new Choice { Label = "Check the config values directly", NextNodeId = "check_config" },
                new Choice { Label = "Look at recent commits to PaymentProcessor", NextNodeId = "recent_commits" }
            ]
        },

        new DecisionNode
        {
            Id = "deploy_history",
            Title = "Deploy History",
            Text = "You check the CI/CD dashboard. A deploy went out at 6:45 AM — two minutes before the errors started. The commit message reads: 'Refactor: extract payment config into separate service.' Author: the new hire, Jamie. No PR reviews on it. It was merged to main at 2 AM.",
            Choices =
            [
                new Choice { Label = "Revert Jamie's commit immediately", NextNodeId = "revert_immediately" },
                new Choice { Label = "Read through Jamie's changes first", NextNodeId = "read_jamies_code" },
                new Choice { Label = "Message Jamie directly", NextNodeId = "message_jamie" }
            ]
        },

        new DecisionNode
        {
            Id = "ask_channel",
            Title = "The Channel",
            Text = "You type 'Does anyone know what changed?' Three people respond simultaneously: 'checking...', 'looking into it', and 'I think Jamie deployed something last night.' Your manager replies: 'We need someone to own this. Who's leading the investigation?' Silence.",
            Choices =
            [
                new Choice { Label = "Volunteer to lead the incident", NextNodeId = "lead_incident" },
                new Choice { Label = "Stay quiet and investigate on your own", NextNodeId = "check_logs" }
            ]
        },

        // === ACT 2: Investigation ===

        new DecisionNode
        {
            Id = "check_config",
            Title = "The Config Values",
            Text = "You query the config service API directly. The 'payment_gateway_key' field returns... an empty string. It existed before — you can see it in the audit log. But at 6:45 AM, something overwrote it with ''. The timestamp matches the deploy exactly.",
            Choices =
            [
                new Choice { Label = "Restore the config value manually", NextNodeId = "manual_fix" },
                new Choice { Label = "Find out why the deploy cleared it", NextNodeId = "read_jamies_code" }
            ]
        },

        new DecisionNode
        {
            Id = "recent_commits",
            Title = "The Git Log",
            Text = "You run `git log --oneline -10` on the payments repo. Jamie's commit is there: a 400-line refactor that extracts config loading into a new ConfigProvider class. The old code read directly from environment variables. The new code reads from a config service — but the migration script that populates it wasn't included in the PR.",
            Choices =
            [
                new Choice { Label = "Revert the commit", NextNodeId = "revert_immediately" },
                new Choice { Label = "Write the missing migration script", NextNodeId = "write_migration" },
                new Choice { Label = "Check if there's a feature flag to bypass the new code", NextNodeId = "feature_flag" }
            ]
        },

        new DecisionNode
        {
            Id = "lead_incident",
            Title = "Incident Commander",
            Text = "You type 'I'll lead this.' Your manager sends a thumbs up. You now have 47 people watching your updates. You create a thread: 'Incident Timeline' and start documenting. The CTO joins the channel. Pressure is on. First things first — you need to understand the blast radius.",
            Choices =
            [
                new Choice { Label = "Check how many customers are affected", NextNodeId = "blast_radius" },
                new Choice { Label = "Focus on the fix first, metrics later", NextNodeId = "check_logs" }
            ]
        },

        new DecisionNode
        {
            Id = "read_jamies_code",
            Title = "Jamie's Refactor",
            Text = "The code is actually well-structured — Jamie clearly knows what they're doing. The new ConfigProvider class has good error handling, retry logic, even circuit breaking. But there's one critical oversight: the constructor calls `LoadConfig()` which hits the config service, but the config service doesn't have the values yet because the seeding script was meant to run as a separate deploy step.",
            Choices =
            [
                new Choice { Label = "Run the seeding script manually in prod", NextNodeId = "run_seed_script" },
                new Choice { Label = "Revert and fix the deploy process", NextNodeId = "revert_and_fix" },
                new Choice { Label = "Hotfix: add fallback to env vars", NextNodeId = "hotfix_fallback" }
            ]
        },

        new DecisionNode
        {
            Id = "message_jamie",
            Title = "Reaching Out",
            Text = "You DM Jamie: 'Hey, the payments service is down in prod — looks related to your config refactor. Are you around?' Jamie responds instantly despite it being 7 AM: 'Oh no. I thought that wouldn't deploy until Wednesday. The feature flag was supposed to gate it. Let me check...' Jamie goes silent for two minutes, then: 'The flag defaulted to ON. I set it to OFF in staging but not prod.'",
            Choices =
            [
                new Choice { Label = "Ask Jamie to flip the flag in prod", NextNodeId = "feature_flag" },
                new Choice { Label = "Flip it yourself — faster", NextNodeId = "flip_it_yourself" },
                new Choice { Label = "Revert instead — flags are unreliable", NextNodeId = "revert_immediately" }
            ]
        },

        new DecisionNode
        {
            Id = "flip_it_yourself",
            Title = "Taking the Wheel",
            Text = "You open LaunchDarkly and flip 'use-config-service' to OFF before Jamie can respond. Error rate drops to zero in under a minute. Jamie messages: 'I was just about to — oh, you got it. Thanks.' There's a brief awkward pause. Jamie adds: 'I should have caught this. I'm sorry.' You realize how you respond here matters more than the technical fix.",
            Choices =
            [
                new Choice { Label = "Reassure Jamie — mistakes happen", NextNodeId = "ending_hero" },
                new Choice { Label = "Focus on process, not blame", NextNodeId = "ending_flag_save" }
            ]
        },

        // === ACT 3: The Fix ===

        new DecisionNode
        {
            Id = "revert_immediately",
            Title = "The Revert",
            Text = "You run `git revert HEAD` and push. CI kicks off. The pipeline takes 8 minutes — tests, build, deploy. You watch the progress bar. At minute 6, a test fails: 'ConfigProviderTest.LoadsFromService — expected service call, got environment variable.' Jamie's test is blocking the revert.",
            Choices =
            [
                new Choice { Label = "Skip the failing test and force deploy", NextNodeId = "skip_tests" },
                new Choice { Label = "Delete Jamie's test file and redeploy", NextNodeId = "delete_test" },
                new Choice { Label = "Fix the test to match reverted behavior", NextNodeId = "fix_test_revert" }
            ]
        },

        new DecisionNode
        {
            Id = "manual_fix",
            Title = "Manual Config Restore",
            Text = "You use the config service admin panel to restore 'payment_gateway_key' to its previous value. Within 30 seconds, the error rate drops to zero. The payments service recovers. Crisis over... for now. But the underlying deploy issue still exists. The same thing will happen next time.",
            Choices =
            [
                new Choice { Label = "Write up the incident and propose fixes", NextNodeId = "ending_bandaid" },
                new Choice { Label = "Fix the root cause now while context is fresh", NextNodeId = "revert_and_fix" }
            ]
        },

        new DecisionNode
        {
            Id = "write_migration",
            Title = "Writing the Migration",
            Text = "You write a quick script to seed the config service from environment variables — essentially bridging the old and new approaches. You test it locally against a staging config service. It works. But deploying a new script to prod during an active incident feels risky.",
            Choices =
            [
                new Choice { Label = "Deploy the script to prod", NextNodeId = "run_seed_script" },
                new Choice { Label = "Too risky — just revert for now", NextNodeId = "revert_and_fix" }
            ]
        },

        new DecisionNode
        {
            Id = "feature_flag",
            Title = "The Feature Flag",
            Text = "You find the flag in LaunchDarkly: 'use-config-service' — it's set to ON in production (defaulted when created). You flip it to OFF. The payments service immediately falls back to reading env vars directly. Error rate drops to zero in under a minute. Clean fix, no deploy needed.",
            Choices =
            [
                new Choice { Label = "Celebrate and write up the incident", NextNodeId = "ending_flag_save" },
                new Choice { Label = "Investigate why the flag defaulted to ON", NextNodeId = "flag_investigation" }
            ]
        },

        new DecisionNode
        {
            Id = "run_seed_script",
            Title = "Running the Seed Script",
            Text = "You run the migration against prod. It populates 12 config values from their environment variable equivalents. The config service now has everything the new code needs. Error rate drops to zero. Jamie's refactor is now working correctly in production — the way it was intended to.",
            Choices =
            [
                new Choice { Label = "Document the deploy gap and move on", NextNodeId = "ending_proper_fix" },
                new Choice { Label = "Add the seed script to the CI pipeline", NextNodeId = "ending_hero" }
            ]
        },

        new DecisionNode
        {
            Id = "hotfix_fallback",
            Title = "The Hotfix",
            Text = "You add three lines to ConfigProvider: if the config service returns empty, fall back to the environment variable. You push, CI passes (all tests still work because the fallback is transparent), deploy goes out. Error rate drops to zero. It's elegant — the new system works when ready, old system covers gaps.",
            Choices =
            [
                new Choice { Label = "Ship it and write up the incident", NextNodeId = "ending_hero" },
                new Choice { Label = "Add alerting for when the fallback triggers", NextNodeId = "ending_proper_fix" }
            ]
        },

        new DecisionNode
        {
            Id = "revert_and_fix",
            Title = "Revert and Proper Fix",
            Text = "You coordinate with Jamie: revert now, then re-land the change with the migration script bundled as a pre-deploy step. Jamie creates a PR within an hour — this time with the seed script, a deployment runbook, and a feature flag set to OFF by default. Two reviewers approve it.",
            Choices =
            [
                new Choice { Label = "Merge and deploy the fixed version", NextNodeId = "ending_proper_fix" }
            ]
        },

        new DecisionNode
        {
            Id = "blast_radius",
            Title = "Blast Radius",
            Text = "You pull up the payments dashboard. 2,847 transactions failed in the last 20 minutes. Revenue impact: ~$184,000 in failed charges. Most will auto-retry, but some customers are seeing error pages. Support tickets are climbing. The CTO posts: 'ETA on fix?'",
            Choices =
            [
                new Choice { Label = "Reply with an honest estimate", NextNodeId = "honest_eta" },
                new Choice { Label = "Say 'working on it' and focus on the fix", NextNodeId = "check_logs" }
            ]
        },

        new DecisionNode
        {
            Id = "honest_eta",
            Title = "The ETA",
            Text = "You type: '15-20 minutes for a revert, faster if we can find a config-level fix.' The CTO responds: 'Do whatever is fastest.' Your manager adds: 'You've got full authority — skip approvals if needed.' That's the green light.",
            Choices =
            [
                new Choice { Label = "Go for the fast config fix", NextNodeId = "manual_fix" },
                new Choice { Label = "Revert — it's more certain", NextNodeId = "revert_immediately" }
            ]
        },

        new DecisionNode
        {
            Id = "skip_tests",
            Title = "Skipping Tests",
            Text = "You push with `--no-verify` and trigger a manual deploy pipeline that skips tests. The deploy goes out. Error rate drops to zero. The incident is over. But now you have a deploy in production that hasn't passed CI, and a precedent that tests can be skipped during incidents.",
            Choices =
            [
                new Choice { Label = "Immediately re-run CI to validate", NextNodeId = "validate_after_skip" },
                new Choice { Label = "It's fine, write up the incident", NextNodeId = "ending_cowboy" }
            ]
        },

        new DecisionNode
        {
            Id = "validate_after_skip",
            Title = "Validating After the Fact",
            Text = "You trigger a full CI run against the deployed commit. It passes — the revert is clean. You update the incident channel: 'Deploy validated post-hoc, all tests green.' Your manager nods. It was a cowboy move, but you cleaned up after yourself. In the retro, you propose adding a 'break glass' CI bypass that auto-triggers validation after deploy.",
            Choices =
            [
                new Choice { Label = "Write up the incident with the proposal", NextNodeId = "ending_proper_fix" }
            ]
        },

        new DecisionNode
        {
            Id = "delete_test",
            Title = "Deleting the Test",
            Text = "You delete ConfigProviderTest.cs, commit with message 'Remove test blocking revert (will re-add)', and push. CI passes. Deploy succeeds. Error rate drops. You make a mental note to re-add that test... later. You add a TODO in the code. Whether it ever gets done is another story.",
            Choices =
            [
                new Choice { Label = "Write up the incident", NextNodeId = "ending_bandaid_test" }
            ]
        },

        new DecisionNode
        {
            Id = "fix_test_revert",
            Title = "Fixing the Test",
            Text = "You update the test to assert the old behavior (reading from env vars). It takes 4 minutes. CI passes on the next run. Full deploy pipeline: green. The revert goes live. Error rate drops to zero. Everything is clean — no skipped tests, no deleted code, no hacks.",
            Choices =
            [
                new Choice { Label = "Write up the incident", NextNodeId = "ending_proper_fix" }
            ]
        },

        new DecisionNode
        {
            Id = "flag_investigation",
            Title = "Flag Defaults",
            Text = "You dig into LaunchDarkly. The flag was created with 'default: true' because Jamie set it up in a dev environment where they wanted it on. When the flag synced to prod, the default carried over. There's no org-wide policy for flag defaults. You realize this could happen to any team.",
            Choices =
            [
                new Choice { Label = "Propose a 'flags default OFF in prod' policy", NextNodeId = "ending_hero" },
                new Choice { Label = "Just document it in the post-mortem", NextNodeId = "ending_flag_save" }
            ]
        },

        // === ENDINGS ===

        new DecisionNode
        {
            Id = "ending_hero",
            Title = "The Hero",
            Text = "You not only fixed the incident but improved the system. Your post-mortem identifies the gap, your fix prevents recurrence, and you've earned trust from the team. Jamie sends you a DM: 'Thanks for not making me feel terrible about this. I learned a lot today.' Your manager mentions it in the next team retro as an example of great incident response. Tuesday starts much quieter."
        },

        new DecisionNode
        {
            Id = "ending_proper_fix",
            Title = "By the Book",
            Text = "Clean revert, proper fix, thorough post-mortem. The incident lasted 25 minutes. The post-mortem action items actually get completed: deploy pipeline now requires migration scripts, PRs need a deploy checklist, and late-night merges require a second reviewer. Not glamorous, but the system is stronger. You go back to your coffee — it's cold now."
        },

        new DecisionNode
        {
            Id = "ending_flag_save",
            Title = "The Quick Save",
            Text = "Feature flag to the rescue — 90 seconds from alert to resolution. Your post-mortem praises the flag infrastructure. But privately you think: 'We got lucky this time. What if there hadn't been a flag?' The underlying deploy process issue lives to cause another incident another day. You file a ticket. It sits in the backlog."
        },

        new DecisionNode
        {
            Id = "ending_bandaid",
            Title = "The Band-Aid",
            Text = "The bleeding stopped but the wound isn't healed. You restored the config value manually, which solved the immediate problem — but the deploy pipeline still has no guardrails. A month later, a similar incident happens with a different service: a deploy overwrites config with no migration step. The post-mortem references yours: 'Same root cause — see incident #4471.' Your manager sighs."
        },

        new DecisionNode
        {
            Id = "ending_bandaid_test",
            Title = "The Missing Test",
            Text = "The revert went out and production recovered. But that deleted test file lingers in the git history like a guilty conscience. You told yourself you'd re-add it. Three sprints later, someone refactors ConfigProvider again — and introduces the exact bug Jamie's test would have caught. The incident channel lights up for the second time. You stare at your TODO comment in the code and wonder if 'later' ever really comes."
        },

        new DecisionNode
        {
            Id = "ending_cowboy",
            Title = "The Cowboy",
            Text = "You got it done fast — production is up, customers are happy, revenue is flowing. But the team noticed you skipped tests and safety rails. In the retro, someone asks: 'What if the revert had introduced a different bug?' There's no good answer. You write a runbook for next time: 'How to safely skip CI during incidents.' It's three pages long. Maybe the tests were faster after all."
        }
    ];
}
