%% version modified for show 2 rotations and then choose

%% generate settings file for chooseyourtarget
FBDur = 1;
HoldTime = 0.25;
RandHold = 0.5;
ChoiceInstructions = "Select rotation [1] or [2]";

output = cell(4, 2);
output{1,1} = "FBDur";
output{2,1} = "HoldTime";
output{3,1} = "RandHold";
output{4,1} = "ChoiceInstructions";
output{1,2} = FBDur;
output{2,2} = HoldTime;
output{3,2} = RandHold;
output{4,2} = ChoiceInstructions;

cd('C:\Users\kimol\Desktop\ChooseYourTarget\LoadIn\SessionSetups')
T = cell2table(output);
writetable(T,'S02Settings.csv','WriteVariableNames',0,'QuoteStrings',false)

%% generate trial file for chooseyourtarget
% TODO: set this up to randomize later
dir90tut = 1;

referenceTarget = 90;
possibleTargets = [0,30,60,120,150,180,210,240,270,300,330];
tutorialAll90 = [2, 1];
tutorialAllPossible = [16, 3];
testBlock = [1296, 19];
totalTrials = tutorialAll90(1) + tutorialAllPossible(1) + testBlock(1);

rotn = 10:10:90;
tutRotn = [10:10:80];
possiblePairs = [10, 20;...
    10, 30;...
    10, 40;...
    10, 50;...
    10, 60;...
    10, 70;...
    10, 80;...
    10, 90;...
    20, 30;...
    20, 40;...
    20, 50;...
    20, 60;...
    20, 70;...
    20, 80;...
    20, 90;...
    30, 40;...
    30, 50;...
    30, 60;...
    30, 70;...
    30, 80;...
    30, 90;...
    40, 50;...
    40, 60;...
    40, 70;...
    40, 80;...
    40, 90;...
    50, 60;...
    50, 70;...
    50, 80;...
    50, 90;...
    60, 70;...
    60, 80;...
    60, 90;...
    70, 80;...
    70, 90;...
    80, 90];
possiblePairs = [possiblePairs, [1:length(possiblePairs)]']; % let the 3rd column be a flag indicating which combination of rotation magnitudes is contained in the row
temp = [possiblePairs(:,2), possiblePairs(:,1), possiblePairs(:,3)];
possiblePairs = [possiblePairs; temp];
possiblePairs = [possiblePairs; possiblePairs * -1];
possiblePairs(:,3) = abs(possiblePairs(:,3));
delay = 1;

rotation = nan(1, totalTrials);
targetAngles = nan(1, totalTrials);
choices = nan(1, totalTrials);
targetTags = cell(1, totalTrials);
feedbackDelays = nan(1, totalTrials);
instructionStrings = cell(1, totalTrials);

% set up tutorial with all 90 degree rotations
% increment loop by 2 because all trials are set up in pairs
index = tutorialAll90(2);
for i = tutorialAll90(2) : 2 : tutorialAll90(2) + tutorialAll90(1) - 1
    rotation(index) = 90 * dir90tut;
    targetAngles(index) = referenceTarget;
    choices(index) = 0;
    targetTags{index} = '';
    if index == 1
        instructionStrings{index} = 'The cursor movement will be rotated relative to your movement. Move your hand straight through the target to observe where the cursor goes. The next time a target appears try to hit the target by countering that rotation.';
    else
        instructionStrings{index} = '';
    end
    feedbackDelays(index) = 1;
    index = index + 1;
    
    rotation(index) = 90 * dir90tut;
    targetAngles(index) = referenceTarget;
    choices(index) = 0;
    targetTags{index} = '';
    instructionStrings{index} = '';
    feedbackDelays(index) = 1;
    index = index + 1;
end

% set up tutorial with remaining tutorial angles
% just alternate the appearance of cw and ccw for now
availableTargets = possibleTargets;
availableRotn = tutRotn;
dirNow = dir90tut * -1;
for i = tutorialAllPossible(2) : 2 : tutorialAllPossible(2) + tutorialAllPossible(1) - 1
    [rotation(index), availableRotn] = ...
        selectOneWithoutReplacement(availableRotn, tutRotn);
    rotation(index) = rotation(index) * dirNow;
    targetAngles(index) = referenceTarget;
    choices(index) = 0;
    targetTags{index} = 'Observe';
    if i == tutorialAllPossible(2)
        instructionStrings{index} = "Good job! The second target will start appearing in different locations. Use what you learn by reaching directly towards the straight-ahead targets to counter the rotation and try to hit the other targets with the cursor.";
    else
        instructionStrings{index}="";
    end
    feedbackDelays(index) = 1;
    index = index + 1;
    
    rotation(index) = rotation(index - 1);
    [targetAngles(index), availableTargets] = ...
        selectOneWithoutReplacement(availableTargets, possibleTargets);
    choices(index) = 0;
    targetTags{index} = 'Compensate';
    instructionStrings{index}="";
    feedbackDelays(index) = 1;
    index = index + 1;
    
    dirNow = dirNow * -1;
end

% set up test block
availableRotn = rotn;
possibleMagIDs = 1:36;
for i = 1 : 3 % this is hard coded for now
    bigFail = 1;
    while bigFail == 1
        bigFail = 0;
        block = [];

        % reset the available pairs after each block
        availablePairs = possiblePairs;

        % set up and shuffle miniblocks
        for mb = 1:4
            % set up a miniblock (1 cycle through all the possible magnitude IDs in
            % the third column of possiblePairs)
            %   Note: not possible to fail with this sampling strategy, but the
            % shuffles might fail. Use a separate loop for shuffling accordingly.
            availableMagIDs = possibleMagIDs;
            countCW = 0;
            countCCW = 0;
            miniblock = [];
            for j = 1:length(possibleMagIDs)
                [thisMagID, availableMagIDs] = ...
                    selectOneWithoutReplacement(availableMagIDs, possibleMagIDs);

                % get subset of availablePairs that have the indicated MagID
                if sum(availablePairs(:,3) == thisMagID) == 0
                    bigFail = 1;
                    break;
                end
                
                pairSubset = availablePairs(availablePairs(:,3) == thisMagID, :);

                % get rid of options with a particular direction if they have been
                % oversampled
                if countCW == 18 % get rid of all the possible items with a negative rotation
                    pairSubset = pairSubset(pairSubset(:,1) > 0, :);
                elseif countCCW == 18 % get rid of all the possible items with a positive rotation
                    pairSubset = pairSubset(pairSubset(:,1) < 0, :);
                end
                

                % select one option
                [rows, cols] = size(pairSubset);
                if rows == 0
                    bigFail = 1;
                    break;
                end
                idx = ceil(rand(1) * rows);
                selection = pairSubset(idx, :);

                % update rolling counts
                miniblock = [miniblock; selection];
                if selection(1) > 0
                    countCCW = countCCW + 1;
                else
                    countCW = countCW + 1;
                end

                % remove selected pair from the list
                availablePairs = removeRow(selection, availablePairs);
            end

            fail = 1;
            consecMagThresh = 2;
            while fail == 1
                fail = 0;
                shuffled = [];

                % try randomly shuffling the features that need to be chosen on the
                % next trial
                needCW = 18;
                needCCW = 18;
                haveCW = 0;
                haveCCW = 0;
                consecSameDir = 1;
                prevDir = 0;
                consecSameA = 1;
                consecSameB = 1;
                trackA = 0;
                trackB = 0;
                availableMiniblockPairs = miniblock;
                for m = 1:length(miniblock)
                    trackCol1 = 1;
                    trackCol2 = 1;

                    % pick direction
                    if haveCW == needCW
                        thisDir = 1;
                    elseif haveCCW == needCCW
                        thisDir = -1;
                    else
                        tryAgain = 1;
                        while tryAgain == 1
                            temp = randi([1, 2], 1);
                            if temp == 1
                                thisDir = -1;
                            else
                                thisDir = 1;
                            end
                            if consecSameDir == 3 && thisDir == prevDir
                                tryAgain = 1;
                            else
                                tryAgain = 0;
                            end
                        end
                    end
                    if thisDir == prevDir
                        consecSameDir = consecSameDir + 1;
                    end
                    prevDir = thisDir;
                    if consecSameDir > 3 % not possible to get directions right
                        fail = 1;
                        break
                    end

                    if thisDir == 1
                        haveCCW = haveCCW + 1;
                    else
                        haveCW = haveCW + 1;
                    end


                    % get subset of available trial pairs belonging to this
                    % direction
                    thisDirPairs = availableMiniblockPairs(sign(availableMiniblockPairs(:,1)) == thisDir, :);

                    % select one option
                    [rows, cols] = size(thisDirPairs);
                    if rows == 0
                        bigFail = 1;
                        break;
                    end
                    idx = ceil(rand(1) * rows);
                    selection = thisDirPairs(idx, :);


                    % confirm that the magnitudes haven't been repeated too
                    % much
                    if (trackA == selection(1))
                        consecSameA = consecSameA + 1;
                        protectA = 1;
                        trackCol1 = 0;
                    elseif (trackA == selection(2))
                        consecSameA = consecSameA + 1;
                        protectA = 1;
                        trackCol2 = 0;
                    else
                        protectA = 0;
                    end
                    if consecSameA > consecMagThresh
                        fail = 1;
                        %disp('Random shuffle fail: ANGLE MAG');
                        break;
                    end

                    if (trackB == selection(1))
                        consecSameB = consecSameB + 1;
                        protectB = 1;
                        trackCol1 = 0;
                    elseif (trackB == selection(2))
                        consecSameB = consecSameB + 1;
                        protectB = 1;
                        trackCol2 = 0;
                    else
                        protectB = 0;
                    end
                    if consecSameB > consecMagThresh
                        fail = 1;
                        %disp('Random shuffle fail: ANGLE MAG');
                        break;
                    end

                    if (trackCol1 == 1) && (trackCol2==1) % no matches were found
                        trackA = selection(1);
                        trackB = selection(2);
                        consecSameA = 1;
                        consecSameB = 1;
                    elseif trackCol1 == 1 % no need to track col 2
                        if protectA==1
                            trackB = selection(1);
                            consecSameB = 1;
                        else
                            trackA = selection(1);
                            consecSameA = 1;
                        end
                    elseif trackCol2 == 1 % no need to track col 1
                        if protectA==1
                            trackB = selection(2);
                            consecSameB = 1;
                        else
                            trackA = selection(2);
                            consecSameA = 1;
                        end
                    end

                    % append selection to shuffled miniblock
                    shuffled = [shuffled; selection];

                    % remove selection from available list
                    availableMiniblockPairs = removeRow(selection, availableMiniblockPairs);
                end
            end
            disp('trial set found!');

            % now append to block
            block = [block; shuffled];
        end
    end
    
    %now, add the block randomized trials to the trial order arrays
    % rememebr, for each line in block, there are 3 trials
    for t = 1:length(block)
        % first trial: sample 1
        rotation(index) = block(t,1);
        targetAngles(index) = 90;
        choices(index) = 0;
        targetTags{index} = '1';
        if index == testBlock(2)
            instructionStrings{index}="You're doing great! From now on: You will see 2 different rotations on 2 consecutive trials. Reach straight to the target to find out what rotations are being applied. On the third trial you will choose which rotation you would like to try to overcome (the first [1] or the second [2]).";
        else
            instructionStrings{index}="";
        end
        feedbackDelays(index) = 1;
        index = index + 1;
        
        % second trial: sample 2
        rotation(index) = block(t,2);
        targetAngles(index) = 90;
        choices(index) = 0;
        targetTags{index} = '2';
        instructionStrings{index}="";
        feedbackDelays(index) = 1;
        index = index + 1;
        
        % third trial: choice
        rotation(index) = 0;
        [targetAngles(index), availableTargets] = ...
            selectOneWithoutReplacement(availableTargets, possibleTargets);
        choices(index) = 1;
        targetTags{index} = '';
        instructionStrings{i}='';
        feedbackDelays(index) = 1;
        index = index + 1;
    end
end

instructionStrings{3} = "Good job! The second target will start appearing in different locations. Use what you learn by reaching directly towards the straight-ahead targets to counter the rotation and try to hit the other targets with the cursor.";


% now turn the vectors into a big table
output = cell(6, totalTrials+1);
output{1,1} = "rotations";
output{2,1} = "targetAngles";
output{3,1} = "choices";
output{4,1} = "targetTag";
output{5,1} = "feedbackDelays";
output{6,1} = "instructionStrings";
for t = 1:totalTrials
    output{1, t+1} = rotation(t);
    output{2, t+1} = targetAngles(t);
    output{3, t+1} = choices(t);
    output{4, t+1} = targetTags(t);
    output{5, t+1} = feedbackDelays(t);
    output{6, t+1} = instructionStrings{t};
end

T = cell2table(output);
writetable(T,'S02Orders.csv','WriteVariableNames',0,'QuoteStrings',false)

