﻿namespace Economy.scripts.Messages
{
    using ProtoBuf;
    using Sandbox.Game.Entities;
    using Sandbox.ModAPI;
    using VRage.Game.Entity;
    using VRageMath;

    [ProtoContract]
    public class MessageClientSound : MessageBase
    {
        [ProtoMember(1)]
        public string CueName;

        [ProtoMember(2)]
        public float Volume;

        // Need to hold onto the emitter.
        private MyEntity3DSoundEmitter _emitter;

        public override void ProcessClient()
        {
            MyEntity entity = (MyEntity)MyAPIGateway.Session?.Player?.Character ?? (MyEntity)MyAPIGateway.Session?.ControlledObject?.Entity;

            if (entity == null)
                return;

            // Create a new emitter only if we need a new one.
            if (_emitter == null)
                _emitter = new MyEntity3DSoundEmitter(entity);
            else if (_emitter.Entity != entity)
            {
                // must cleanup the old emitter if the entity has changed.
                _emitter.Cleanup();
                _emitter = new MyEntity3DSoundEmitter(entity);
            }

            MySoundPair soundId = new MySoundPair(CueName);
            _emitter.CustomVolume = Volume;
            _emitter.PlaySound(soundId, true, false, true, false, false);
        }

        public override void ProcessServer()
        {
            // never processed on server.
        }

        /// <summary>
        /// Plays the specified Audio cue from the Audio.sbc definitions to the specifed player.
        /// This is called by the server side to run on the client.
        /// </summary>
        /// <param name="steamId">The id of the player to hear the message.</param>
        /// <param name="cueName">The Audio cue to play.</param>
        /// <param name="volume">The audio volume.</param>
        public static void SendMessage(ulong steamId, string cueName, float volume = 1.0f)
        {
            ConnectionHelper.SendMessageToPlayer(steamId, new MessageClientSound { CueName = cueName, Volume = volume });
        }

        /// <summary>
        /// Play sound from particular location. Example economy LCD when trade zone detected - not working yet 
        /// Code created with assistance of Digi.
        /// </summary>
        /// <param name="soundName"></param>
        /// <param name="position"></param>
        /// <param name="volume"></param>
        public static void PlaySoundFrom(string soundName, Vector3D position, float volume = 1f)
        {
            var emitter = new MyEntity3DSoundEmitter(null)
            {
                CustomVolume = volume,
                CustomMaxDistance = 100
            };
            emitter.SetPosition(position);

            emitter.PlaySingleSound(new MySoundPair(soundName));
            //emitter.PlaySoundWithDistance(new MyCueId());  // MyCueId is not whitelisted and we need a MySoundPair version.
            emitter.Cleanup();

            // This is what we should use, but it doesn't work. At all.
            //Sandbox.Game.MyVisualScriptLogicProvider.CreateSoundEmitterAtPosition(position.ToString(), position);
            //Sandbox.Game.MyVisualScriptLogicProvider.PlaySound(position.ToString(), soundName, false);
        }

        /// <summary>
        /// Play sound to local player only
        /// </summary>
        /// <param name="soundName">The Audio cue to play.</param>
        /// <param name="volume">The audio volume.</param>
        public static void PlaySound(string soundName, float volume = 1.0f)
        {
            var controlled = MyAPIGateway.Session?.ControlledObject?.Entity;

            if (controlled == null)
                return; // don't continue if session is not ready or player does not control anything.

            var emitter = new MyEntity3DSoundEmitter((MyEntity)controlled) { CustomVolume = volume };
            emitter.PlaySingleSound(new MySoundPair(soundName));
            emitter.Cleanup();

            // This is what we should use, but it doesn't work. The entityName comes back blank.
            //string entityName = Sandbox.Game.MyVisualScriptLogicProvider.GetEntityName(controlled.EntityId);
            //Sandbox.Game.MyVisualScriptLogicProvider.CreateSoundEmitterAtEntity("player", entityName);
            //Sandbox.Game.MyVisualScriptLogicProvider.PlaySound("player", soundName, false);
        }
    }
}