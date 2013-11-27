<?php

use Illuminate\Database\Schema\Blueprint;
use Illuminate\Database\Migrations\Migration;

class CreateGroupTextPivot extends Migration {

    /**
     * Run the migrations.
     *
     * @return void
     */
    public function up() {
        Schema::create('group_text', function(Blueprint $table) {
            $table->integer('text_id')->unsigned();
            $table->integer('group_id')->unsigned();

            $table->foreign('text_id')->references('id')->on('texts');
            $table->foreign('group_id')->references('id')->on('groups');
        });
    }

    /**
     * Reverse the migrations.
     *
     * @return void
     */
    public function down() {
        Schema::drop('group_text');
    }

}
