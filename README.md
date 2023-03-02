# FlyTextFilter

Filter fly texts and pop-up texts based on multiple criteria and allow the adjustment of their positions/scaling.

Note: Fly texts and pop-up texts are technically not the same thing, but in the plugin, a fly text usually refers to both, with the exception of scaling.

## Types
Fly texts are categorized in different types.
You can preview each type and its associated styles by using the eye button. Every type has up to 4 different styles, 2 fly texts and 2 pop-up texts.
The filtering is based on source and target, spread in 3 different groups:
<ul>
  <li>(Y)ou - Only from or to you.</li>
  <li>(P)arty - Only from or to your party members, excluding you.</li>
  <li>(O)thers - From or to everyone, excluding you and your party members.</li>
</ul>
The main table can be a bit confusing at first.
The top part (You - Party - Others) is the source, and every letter underneath refers to the target.
For example:
<ul>
  <li>the column Y under You refers to the fly texts from you to you.</li>
  <li>the column P under You refers to the fly texts from you to party members.</li>
  <li>the column Y under Party refers to the fly texts from your party members to you.</li>
</ul>

If you are still confused, an edit button is available at the end of every line. 

![types](https://github.com/Aireil/FlyTextFilter/raw/master/res/types.png)

## Text blacklist
Filter all fly texts containing an entire word. For example, if you add `Embrace`, the specific embrace fly text from SCHs fairies will be filtered.

## Log
To easily identity the fly text type, you can enable logging in the log tab.
This will save all generated fly texts, so you can replay them on you and edit their settings.

The logging setting is turned off every restart by design, to avoid useless processing.

## Adjustments
Ability to hide damage type icons based on type of fly texts (auto attacks, status effects, other damage).
Adjust fly texts positions and fly/pop-up texts scaling.
If the distinction between fly and pop-up text is not clear, you can use the `Test` button and change the scaling.

![scaling](https://github.com/Aireil/FlyTextFilter/raw/master/res/scaling.png)
