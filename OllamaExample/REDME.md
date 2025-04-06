
Run model CPU only [Details](https://ollama.com/blog/ollama-is-now-available-as-an-official-docker-image)

```shell
podman run -d -v ollama:/root/.ollama -p 11434:11434 --name ollama ollama/ollama
# to run specific model
podman exec -it ollama ollama run llama3.1
# or from the podman console of the ollama container run 
ollama run llama3.1
```

[How to download model if it rolling back each time](https://github.com/ollama/ollama/issues/8484) 
It stops when an SSD is performing a write to the disk while being cached on memory first and the disk is busy, so a Ctrl+C is not usable in this case. I wonder if the 5 sec can be altered at run time.

parameters of the model

```
    load_tensors:          CPU model buffer size =  4685.30 MiB
    llama_init_from_model: n_seq_max     = 4
    llama_init_from_model: n_ctx         = 8192
    llama_init_from_model: n_ctx_per_seq = 2048
    llama_init_from_model: n_batch       = 2048
    llama_init_from_model: n_ubatch      = 512
    llama_init_from_model: flash_attn    = 0
    llama_init_from_model: freq_base     = 500000.0
    llama_init_from_model: freq_scale    = 1
    llama_init_from_model: n_ctx_per_seq (2048) < n_ctx_train (131072) -- the full capacity of the model will not be utilized
    llama_kv_cache_init: kv_size = 8192, offload = 1, type_k = 'f16', type_v = 'f16', n_layer = 32, can_shift = 1
    llama_kv_cache_init:        CPU KV buffer size =  1024.00 MiB
    llama_init_from_model: KV self size  = 1024.00 MiB, K (f16):  512.00 MiB, V (f16):  512.00 MiB
    llama_init_from_model:        CPU  output buffer size =     2.02 MiB
    llama_init_from_model:        CPU compute buffer size =   560.01 MiB
```