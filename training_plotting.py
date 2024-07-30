import numpy as np
from stable_baselines3 import PPO
import torch as th
import cmath
from gym_unity.envs import UnityToGymWrapper
from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel
from stable_baselines3.common.callbacks import BaseCallback
import sys
import os

# Custom callback to log rewards
class RewardLoggingCallback(BaseCallback):
    def __init__(self, log_file, verbose=0):
        super(RewardLoggingCallback, self).__init__(verbose)
        self.log_file = log_file
        self.rewards = []
        self.episode_rewards = []

    def _on_step(self) -> bool:
        reward = self.locals['rewards']
        self.episode_rewards.append(reward)
        if self.locals['dones']:
            self.rewards.append(np.sum(self.episode_rewards))
            self.episode_rewards = []
            with open(self.log_file, 'a') as file:
                file.write(f"{self.num_timesteps},{np.mean(self.rewards)}\n")
        return True

# Setup
env_name = r"C:\Users\samhi\RL_for_bifurcation\3DPos.exe"
log_file = "reward_log.txt"

if os.path.exists(log_file):
    os.remove(log_file)

channel = EngineConfigurationChannel()
unity_env = UnityEnvironment(env_name, side_channels=[channel])
channel.set_configuration_parameters(time_scale=2)
env = UnityToGymWrapper(unity_env)

policy_kwargs = dict(activation_fn=th.nn.ReLU,
                     net_arch=[dict(pi=[128, 256, 256, 128], vf=[128, 256, 256, 128])])

reward_callback = RewardLoggingCallback(log_file=log_file)

# Training
model = PPO("MlpPolicy", env, verbose=2, tensorboard_log="./logs_graphs/PPO_3DSim_5000000_new_4_3_versiontrial290724/", policy_kwargs=policy_kwargs)
model.learn(total_timesteps=5000000, reset_num_timesteps=True, tb_log_name="1", callback=[reward_callback])
model.save("trained_models/sim_model_PPO3_versiontrial290724")
