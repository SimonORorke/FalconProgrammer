--[[
    Controls a program-level DAHDSR's attack, decay, sustain and release
    parameters with macros defined in the standard GUI, avoiding the
    need for a script-defined GUI.
    ********************************************************************
    To make this work, there are ADSR knobs on the script processor 
    itself (shown on Falcon's Events page). These script processor knobs
    need to be configured to be modulated by the corresponding macros.
    ********************************************************************
    The reason why the DAHDSR's parameters need to be controlled via a
    script is that there is no other way of scaling macro values 
    (range 0 to 1) up to required parameter value maxima greater than 
    one. For example, for the DAHDSR's ReleaseTime parameter, the 
    maximum value is 20 seconds, though I've set my own maximum at 2 
    seconds.
    There are no examples in UVI-supplied sound banks of programs where
    macros control a DAHDSR's attack, decay, sustain and release without
    requiring a script. That suggests that it really is impossible to do
    otherwise.
    ********************************************************************
    A problem still to solve
    ********************************************************************
    Reducing the attack time to zero while playing intermittently causes
    low volume and then silence. But I have not found the problem to 
    always occur with any specific program. When the problem happens, 
    sometimes Falcon then becomes silent altogether, even when switching 
    to a different program. If Falcon is then restarted, sound comes back.
]]

-- Constants
local AttackMacroName = "Macro 5"
local DahdsrName = "DAHDSR 1"
local DecayMacroName = "Macro 6"
local ReleaseMacroName = "Macro 8"
local SustainMacroName = "Macro 7"

-- Variables
local AttackKnobValue
local Dahdsr = Program.modulations[DahdsrName]
local DecayKnobValue
-- DAHDSR's maximum AttackTime is 10 seconds.
-- But, for my requirements, that's too much and unergonomic.
local MaxAttackSeconds = 1
-- DAHDSR's maximum DecayTime is 30 seconds.
-- But, for my requirements, that's too much and unergonomic.
-- 15 seconds covers the DecayTime values in all but 1 
-- (Nature\Whispering Waters) of the Organic Pads original programs.
local MaxDecaySeconds = 15
-- DAHDSR's maximum ReleaseTime is 20 seconds.
-- But, for my requirements, that's too much and unergonomic.
local MaxReleaseSeconds = 2
local ReleaseKnobValue
local SustainKnobValue

-- GUI
local AttackKnob = Knob("Attack", 0, 0, 1)
local DecayKnob = Knob("Decay", 0, 0, 1)
local SustainKnob = Knob("Sustain", 0, 0, 1)
local ReleaseKnob = Knob("Release", 0, 0, 1)
local MaxAttackSecondsKnob = Knob(
        "MaxAttackSeconds", MaxAttackSeconds, 0, 10)
MaxAttackSecondsKnob.displayName = "Max Attack Secs"
-- Enough to show full display name full size
MaxAttackSecondsKnob.width = 140
MaxAttackSecondsKnob.x = AttackKnob.x
MaxAttackSecondsKnob.y = 55
local MaxDecaySecondsKnob = Knob(
        "MaxDecaySeconds", MaxDecaySeconds, 0, 30)
MaxDecaySecondsKnob.displayName = "Max Decay Secs"
-- Enough to show full display name full size
MaxDecaySecondsKnob.width = 140
MaxDecaySecondsKnob.x = DecayKnob.x + 50
MaxDecaySecondsKnob.y = 55
local MaxReleaseSecondsKnob = Knob(
        "MaxReleaseSeconds", MaxReleaseSeconds, 0, 20)
MaxReleaseSecondsKnob.displayName = "Max Release Secs"
-- Enough to show full display name full size
MaxReleaseSecondsKnob.width = 140
MaxReleaseSecondsKnob.x = ReleaseKnob.x
MaxReleaseSecondsKnob.y = 55

AttackKnob.changed = function(self)
    -- print("AttackKnob.changed")
    AttackKnobValue = onAdsrKnobOrMacroValueChanged(
            self, AttackKnobValue, "AttackTime", MaxAttackSeconds)
end

DecayKnob.changed = function(self)
    -- print("DecayKnob.changed")
    DecayKnobValue = onAdsrKnobOrMacroValueChanged(
            self, DecayKnobValue, "DecayTime", MaxDecaySeconds)
end

MaxAttackSecondsKnob.changed = function(self)
    MaxAttackSeconds = round(self.value, 2)
    print("MaxAttackSecondsKnob.changed: MaxAttackSeconds = " .. MaxAttackSeconds)
end

MaxDecaySecondsKnob.changed = function(self)
    MaxDecaySeconds = round(self.value, 2)
    print("MaxDecaySecondsKnob.changed: MaxDecaySeconds = " .. MaxDecaySeconds)
end

MaxReleaseSecondsKnob.changed = function(self)
    MaxReleaseSeconds = round(self.value, 2)
    print("MaxReleaseSecondsKnob.changed: MaxReleaseSeconds = " .. MaxReleaseSeconds)
end

ReleaseKnob.changed = function(self)
    -- print("ReleaseKnob.changed")
    ReleaseKnobValue = onAdsrKnobOrMacroValueChanged(
            self, ReleaseKnobValue, "ReleaseTime", MaxReleaseSeconds)
end

SustainKnob.changed = function(self)
    -- print("SustainKnob.changed")
    SustainKnobValue = onAdsrKnobOrMacroValueChanged(
            self, SustainKnobValue, "SustainLevel", 1)
end

function initialiseMacroAndKnobFromDahdsr(
        macroName, parameterName, maxParameterValue)
    local macro = Program.modulations[macroName]
    if macro == nil then
        error("Cannot find macro '" .. macroName ..
                "' to modulate DAHDSR parameter '" .. parameterName .. "'.")
    end
    local parameterValue = round(Dahdsr:getParameter(parameterName), 2)
    local scaled = scale(parameterValue, 0, maxParameterValue, 0, 1)
    local knobValue
    if scaled < 1 then
        knobValue = scaled
    else
        knobValue = 1
    end
    -- print("initialiseMacroAndKnobFromDahdsr: parameterName = " .. parameterName .. 
    --     "; maxParameterValue = " .. maxParameterValue ..
    --     "; parameterValue = " .. parameterValue .. "; scaled = " .. scaled ..
    --     "; knobValue = " .. knobValue)
    -- Will set the corresponding script processor knob too,
    -- provided the knob has been configured to be modulated by the macro.
    macro:setParameter("Value", knobValue)
    return knobValue
end

-- This will work when the corresponding macro's value has changed,
-- provided the knob has been configured to be modulated by the macro.
function onAdsrKnobOrMacroValueChanged(
        knob, currentKnobValue, parameterName, maxParameterValue)
    local newKnobValue = round(knob.value, 2)
    if newKnobValue ~= currentKnobValue then
        if currentKnobValue ~= nil then
            -- The knob has not just been initialised from the DAHDSR parameter. 
            setDahdsrParameterValue(
                    parameterName, maxParameterValue, newKnobValue)
        end
    end
    return newKnobValue
end

function onInit()
    -- print("Initialising")
    if Dahdsr == nil then
        error("Cannot find DAHDSR '" .. DahdsrName .. "'.")
    end
    AttackKnobValue = initialiseMacroAndKnobFromDahdsr(
            AttackMacroName, "AttackTime", MaxAttackSeconds)
    DecayKnobValue = initialiseMacroAndKnobFromDahdsr(
            DecayMacroName, "DecayTime", MaxDecaySeconds)
    SustainKnobValue = initialiseMacroAndKnobFromDahdsr(
            SustainMacroName, "SustainLevel", 1)
    ReleaseKnobValue = initialiseMacroAndKnobFromDahdsr(
            ReleaseMacroName, "ReleaseTime", MaxReleaseSeconds)
end

function round(num, numDecimalPlaces)
    local multiplier = 10^(numDecimalPlaces or 0)
    return math.floor(num * multiplier + 0.5) / multiplier
end

function scale(value, fromMin, fromMax, toMin, toMax)
    return
    (value - fromMin) * (toMax - toMin) / (fromMax - fromMin) + toMin;
end

function setDahdsrParameterValue(parameterName, maxParameterValue, knobValue)
    local scaled = scale(knobValue, 0, 1, 0, maxParameterValue)
    -- print("setDahdsrParameterValue: knobValue = " .. knobValue .. 
    -- "; maxParameterValue = " .. maxParameterValue .. 
    -- "; " .. parameterName .. " = " .. scaled)
    Dahdsr:setParameter(parameterName, scaled)
end